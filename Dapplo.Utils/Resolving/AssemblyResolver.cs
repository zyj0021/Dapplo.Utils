﻿#region Dapplo 2016 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2016 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.Utils
// 
// Dapplo.Utils is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.Utils is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.Utils. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapplo.Utils.Embedded;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.Utils.Resolving
{
	/// <summary>
	///     This is a static Assembly resolver and Assembly loader
	///     It doesn't use a logger or other dependencies outside the Dapplo.Utils dll to make it possible to only have this DLL in the output directory
	/// </summary>
	public static class AssemblyResolver
	{
		private static readonly ISet<string> AppDomainRegistrations = new HashSet<string>();
		private static readonly IDictionary<string, Assembly> Assemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///     Setup and Register some of the default assemblies in the assembly cache
		/// </summary>
		static AssemblyResolver()
		{
			Register(Assembly.GetCallingAssembly());
			Register(Assembly.GetEntryAssembly());
			Register(Assembly.GetExecutingAssembly());
			ResolveDirectories.Add(".");
		}

		/// <summary>
		/// Extension to register an assembly to the AssemblyResolver, this is used for resolving embedded assemblies
		/// </summary>
		/// <param name="assembly">Assembly</param>
		public static void Register(this Assembly assembly)
		{
			if (assembly == null)
			{
				return;
			}
			lock (Assemblies)
			{
				if (!Assemblies.ContainsKey(assembly.GetName().Name))
				{
					Assemblies[assembly.GetName().Name] = assembly;
				}
			}
		}

		/// <summary>
		///     IEnumerable with all cached assemblies
		/// </summary>
		public static IEnumerable<Assembly> AssemblyCache => Assemblies.Values;

		/// <summary>
		///     A collection of all directories where the resolver will look to resolve resources
		/// </summary>
		public static ISet<string> ResolveDirectories { get; } = new HashSet<string>();

		/// <summary>
		///     Defines if the resolving is first loading internal files, if nothing was found check the file system
		///     There might be security reasons for not doing this.
		/// </summary>
		public static bool ResolveEmbeddedBeforeFiles { get; set; } = true;

		/// <summary>
		///     Register the AssemblyResolve event for the specified AppDomain
		///     This can be called multiple times, it detect this.
		/// </summary>
		/// <returns>IDisposable, when disposing this the event registration is removed</returns>
		public static IDisposable RegisterAssemblyResolve(this AppDomain appDomain)
		{
			lock (AppDomainRegistrations)
			{
				if (!AppDomainRegistrations.Contains(appDomain.FriendlyName))
				{
					AppDomainRegistrations.Add(appDomain.FriendlyName);
					appDomain.AssemblyResolve += ResolveEventHandler;
				}
				return Disposable.Create(() => UnegisterAssemblyResolve(appDomain));
			}
		}

		/// <summary>
		///     Register AssemblyResolve on the current AppDomain
		/// </summary>
		/// <returns>IDisposable, when disposing this the event registration is removed</returns>
		public static IDisposable RegisterAssemblyResolve()
		{
			return AppDomain.CurrentDomain.RegisterAssemblyResolve();
		}

		/// <summary>
		///     Unegister the AssemblyResolve event for the specified AppDomain
		///     This can be called multiple times, it detect this.
		/// </summary>
		public static void UnegisterAssemblyResolve(this AppDomain appDomain)
		{
			lock (AppDomainRegistrations)
			{
				if (AppDomainRegistrations.Contains(appDomain.FriendlyName))
				{
					AppDomainRegistrations.Remove(appDomain.FriendlyName);
					appDomain.AssemblyResolve -= ResolveEventHandler;
				}
			}
		}

		/// <summary>
		///     Unregister AssemblyResolve from the current AppDomain
		/// </summary>
		public static void UnegisterAssemblyResolve()
		{
			AppDomain.CurrentDomain.UnegisterAssemblyResolve();
		}

		/// <summary>
		///     A resolver which takes care of loading DLL's which are referenced from AddOns but not found
		/// </summary>
		/// <param name="sender">object</param>
		/// <param name="resolveEventArgs">ResolveEventArgs</param>
		/// <returns>Assembly</returns>
		private static Assembly ResolveEventHandler(object sender, ResolveEventArgs resolveEventArgs)
		{
			var assemblyName = new AssemblyName(resolveEventArgs.Name);

			return FindAssembly(assemblyName.Name);
		}

		/// <summary>
		///     Simple method to load an assembly from a file path (or returned a cached version).
		///     If it was loaded new, it will be added to the cache
		/// </summary>
		/// <param name="filepath">string with the path to the file</param>
		/// <returns>Assembly</returns>
		public static Assembly LoadAssemblyFromFile(string filepath)
		{
			var assembly = Assemblies.Values.FirstOrDefault(x => x.Location == filepath);
			if (assembly == null)
			{
				assembly = Assembly.LoadFile(filepath);

				Register(assembly);
			}
			return assembly;
		}

		/// <summary>
		///     Simple method to load an assembly from a stream.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>Assembly or null when the stream is null</returns>
		public static Assembly LoadAssemblyFromStream(Stream stream)
		{
			if (stream == null)
			{
				return null;
			}
			// Load the assembly, unfortunately this only works via a byte array
			var assembly = Assembly.Load(stream.ToByteArray());
			Register(assembly);
			return assembly;
		}

		/// <summary>
		///     Load the specified assembly from a manifest resource or from the file system
		/// </summary>
		/// <param name="assemblyName">string from AssemblyName.Name, do not specify an extension</param>
		/// <returns>Assembly or null</returns>
		public static Assembly FindAssembly(string assemblyName)
		{
			Assembly assembly;
			lock (Assemblies)
			{
				// Try the cache
				if (Assemblies.TryGetValue(assemblyName, out assembly))
				{
					return assembly;
				}
			}

			// Loading order depends on ResolveEmbeddedBeforeFiles
			if (ResolveEmbeddedBeforeFiles)
			{
				assembly = LoadEmbeddedAssembly(assemblyName) ?? LoadAssemblyFromFileSystem(assemblyName);
			}
			else
			{
				assembly = LoadAssemblyFromFileSystem(assemblyName) ?? LoadEmbeddedAssembly(assemblyName);
			}

			return assembly;
		}

		/// <summary>
		///     Load the specified assembly from a manifest resource, or return null
		/// </summary>
		/// <param name="assemblyName">string</param>
		/// <returns>Assembly</returns>
		public static Assembly LoadEmbeddedAssembly(string assemblyName)
		{
			var dllName = $"{assemblyName}.dll";
			try
			{
				var resourceTuple = AssemblyCache.FindEmbeddedResources(dllName).FirstOrDefault();
				if (resourceTuple != null)
				{
					using (var stream = resourceTuple.Item1.GetEmbeddedResourceAsStream(resourceTuple.Item2))
					{
						return LoadAssemblyFromStream(stream);
					}
				}
			}
			catch (Exception ex)
			{
				// don't log with other libraries as this might cause issues / recurse resolving
				Trace.WriteLine($"Error loading {dllName} from manifest resources: {ex.Message}");
			}
			return null;
		}

		/// <summary>
		///     Load the specified assembly from the ResolveDirectories, or return null
		/// </summary>
		/// <param name="assemblyName">string with the name without path</param>
		/// <returns>Assembly</returns>
		public static Assembly LoadAssemblyFromFileSystem(string assemblyName)
		{
			return LoadAssemblyFromFileSystem(ResolveDirectories, assemblyName);
		}

		/// <summary>
		///     Load the specified assembly from the specified directories, or return null
		/// </summary>
		/// <param name="directories">IEnumerable with directories</param>
		/// <param name="assemblyName">string with the name without path</param>
		/// <returns>Assembly</returns>
		public static Assembly LoadAssemblyFromFileSystem(IEnumerable<string> directories, string assemblyName)
		{
			var dllName = $"{assemblyName}.dll";

			var filepath = FileLocations.Scan(ResolveDirectories, dllName).FirstOrDefault();
			if (!string.IsNullOrEmpty(filepath) && File.Exists(filepath))
			{
				try
				{
					return LoadAssemblyFromFile(filepath);
				}
				catch (Exception ex)
				{
					// don't log with other libraries as this might cause issues / recurse resolving
					Trace.WriteLine($"Error loading {filepath} : {ex.Message}");
				}
			}
			return null;
		}
	}
}