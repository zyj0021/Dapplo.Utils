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

using System.Threading.Tasks;
using Dapplo.Log;
using Xunit;
using Xunit.Abstractions;
using Dapplo.Log.XUnit;

#endregion

namespace Dapplo.Utils.Tests
{
	public class AsyncLockTests
	{
		private static readonly LogSource Log = new LogSource();

		public AsyncLockTests(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

		[Fact]
		public async Task TestAsyncLock()
		{
			using (var asyncLock = new AsyncLock())
			{
				using (await asyncLock.LockAsync())
				{
					Log.Debug().WriteLine("Got lock!");
				}
			}
		}

		[Fact]
		public void TestAsyncLockDoubleDispose()
		{
			using (var asyncLock = new AsyncLock())
			{
				// Call dispose inside the using, so it's called twice
				asyncLock.Dispose();
			}
		}

		[Fact]
		public void TestAsyncLockFinalizer()
		{
			// force the finalizer usage
			// ReSharper disable once UnusedVariable
			var asyncLock = new AsyncLock();
		}
	}
}