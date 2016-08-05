﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Utils
// 
//  Dapplo.Utils is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Utils is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Utils. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Dapplo.Utils
{
	/// <summary>
	///     Some utils for working with files (mainly filename)
	/// </summary>
	public static class FileTools
	{
		/// <summary>
		/// Create a regex to find the specified file with wildcards.
		/// </summary>
		/// <param name="filename">
		/// string with the filename pattern to match, like Dapplo.* is allowed.
		/// Together with extensions "dll" and "dll.gz" this would be converted to Dapplo\..*(\.dll|\.dll\.gz)$
		/// (the . in the filename pattern is NOT a any, for this a ? should be used)
		/// </param>
		/// <param name="extensions">Extensions which need to be matched allowed</param>
		/// <param name="ignoreCase">default is true and makes sure the case is ignored</param>
		/// <param name="prefix">
		/// The prefix by default restricts the match to be a complete filename, independend of the path before it.
		/// For resources the predix could be the namespace but you can also specify the directory up to the file if you want a concrete fix.
		/// </param>
		/// <returns>Regex representing the filename pattern</returns>
		public static Regex FilenameToRegex(string filename, IEnumerable<string> extensions, bool ignoreCase = true, string prefix = @"^(.*\\)*")
		{
			if (filename == null)
			{
				throw new ArgumentNullException(nameof(filename));
			}

			if (extensions == null)
			{
				throw new ArgumentNullException(nameof(extensions));
			}
			// 1: Escape all dots
			// 2: Replace all ? with a single dot
			// 3: Replace * for a matching on NOT the path separator, we only want the file

			var regex = new StringBuilder(prefix);
			regex.Append(filename.Replace(".", @"\.").Replace('?', '.').Replace("*", @"[^\\]*"));

			var cleanedExtensions = extensions.Select(e => e.StartsWith(".") ? e.TrimStart('.') : e).ToList();
			if (cleanedExtensions.Count > 1)
			{
				regex.Append('(');
			}
			foreach (var allowedExtension in cleanedExtensions)
			{
				regex.Append(@"\.");
				regex.Append(allowedExtension.Replace(".", @"\."));
				regex.Append('|');
			}
			regex.Length -= 1;
			if (cleanedExtensions.Count > 1)
			{
				regex.Append(')');
			}
			regex.Append('$');
			return new Regex(regex.ToString(), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
		}

		/// <summary>
		/// Remove extensions from a filename / path
		/// </summary>
		/// <param name="filepath">string with a filename or path</param>
		/// <param name="extensions">IEnumerable with extensions to remove</param>
		/// <returns>string</returns>
		public static string RemoveExtensions(string filepath, IEnumerable<string> extensions = null)
		{
			if (extensions == null)
			{
				throw new ArgumentNullException(nameof(extensions));
			}

			var orderedExtensions = from extension in extensions
									orderby extension.Length
									select extension;

			foreach (var extension in orderedExtensions)
			{
				if (filepath.EndsWith(extension))
				{
					filepath = filepath.Replace($".{extension}", "");
				}
			}
			return filepath;
		}
	}
}