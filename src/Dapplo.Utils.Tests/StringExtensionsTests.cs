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

using System.Collections.Generic;
using Dapplo.Utils.Extensions;
using Xunit;
using System;
using Dapplo.Utils.Tests.TestEntities;

#endregion

namespace Dapplo.Utils.Tests
{
	/// <summary>
	/// Tests for StringExtensions
	/// </summary>
	public class StringExtensionsTests
	{
		private const string Expected = "Jan is 10 years old";

		[Fact]
		public void TestFormatWith_Basics()
		{
			var result = "{Name} is {Age} years old".FormatWith(new {Name = "Jan", Age = 10});
			Assert.Equal(Expected, result);

			result = "{0} is {1} years old".FormatWith("Jan", 10);
			Assert.Equal(Expected, result);

			result = "{Name} is {4} years old".FormatWith(new {Name = "Jan", Age = 10}, 1, 2, 3, 10);
			Assert.Equal(Expected, result);
		}

		[Fact]
		public void TestFormatWith_Dictionary()
		{
			var result = "{Name} is {Age} years old".FormatWith(new Dictionary<string, object> {{"Name", "Jan"}, {"Age", 10}});
			Assert.Equal(Expected, result);

			result = "{EnumValue}".FormatWith(new Dictionary<string, object> { { "EnumValue",  TestEnum.Value1 } });
			Assert.Equal("1", result);
			result = "{EnumValue}".FormatWith(new Dictionary<string, object> { { "EnumValue", TestEnum.Value2 } });
			Assert.Equal("Value2", result);
		}

		[Fact]
		public void TestFormatWith_Null()
		{
			string nullString = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<ArgumentNullException>(() => nullString.FormatWith("nothing"));
			var result = "{NullValue}".FormatWith(new Dictionary<string, object> { { "NullValue", null } });
			Assert.Equal("", result);
		}

		[Fact]
		public void TestNonStrictEquals()
		{
			string nullString = null;
			Assert.True("abc123".NonStrictEquals("__AbC_123!"));
			Assert.False("abc123".NonStrictEquals("a__AbC_123!"));
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.True(nullString.NonStrictEquals(null));
		}

		[Fact]
		public void TestRemoveStartEndQuotes()
		{
			const string blub = "blub";
			Assert.Equal(blub, $"\"{blub}\"".RemoveStartEndQuotes());
		}


		[Fact]
		public void TestSplitDictionary()
		{
			const string csList = "value1=1,value2=2";
			var dictionary = csList.SplitDictionary();
			Assert.True(dictionary.ContainsKey("value1"));
			Assert.Equal("1",dictionary["value1"]);

		}

		[Fact]
		public void TestRemoveStartEndQuotesd()
		{
			const string quotedString = "\"blub\"";
			var unquotedString = quotedString.RemoveStartEndQuotes();
			Assert.Equal("blub", unquotedString);

			unquotedString = unquotedString.RemoveStartEndQuotes();
			Assert.Equal("blub", unquotedString);

			string nullString = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Null(nullString.RemoveStartEndQuotes());
		}
	}
}