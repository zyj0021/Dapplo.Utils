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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace Dapplo.Utils
{
	public static class PropertyInfoExtension
	{
#if !_PCL_
		/// <summary>
		///     Retrieve the Category from the CategoryAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Category</returns>
		public static string GetCategory(this PropertyInfo propertyInfo)
		{
			var categoryAttribute = propertyInfo.GetCustomAttribute<CategoryAttribute>(true);
			return categoryAttribute?.Category;
		}
#endif

		/// <summary>
		///     Retrieve the Name from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Name</returns>
		public static string GetDataMemberName(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>(true);
			if (!string.IsNullOrEmpty(dataMemberAttribute?.Name))
			{
				return dataMemberAttribute.Name;
			}
			return null;
		}

		/// <summary>
		///     Create a default for the property.
		///     This can come from the DefaultValueFor from the DefaultValueAttribute
		///     Or it can be something like an empty collection
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>object with a default value</returns>
		public static object GetDefaultValue(this PropertyInfo propertyInfo)
		{
			var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>(true);
			if (defaultValueAttribute != null)
			{
				return defaultValueAttribute.Value;
			}
			if (propertyInfo.PropertyType.GetTypeInfo().IsValueType)
			{
				// msdn information: If this PropertyInfo object is a value type and value is null, then the property will be set to the default value for that type.
				return null;
			}

			try
			{
				return propertyInfo.PropertyType.CreateInstance();
			}
			catch (Exception)
			{
				// Ignore creating the default type, this might happen if there is no default constructor.
			}

			return null;
		}

#if !_PCL_
		/// <summary>
		///     Retrieve the Description from the DescriptionAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Description</returns>
		public static string GetDescription(this PropertyInfo propertyInfo)
		{
			var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>(true);
			return descriptionAttribute?.Description;
		}
#endif

		/// <summary>
		///     Retrieve the EmitDefaultValue from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>EmitDefaultValue</returns>
		public static bool GetEmitDefaultValue(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>(true);
			if (dataMemberAttribute != null)
			{
				return dataMemberAttribute.EmitDefaultValue;
			}
			return false;
		}

#if !_PCL_
		/// <summary>
		///     Retrieve the IsReadOnly from the ReadOnlyAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IsReadOnly</returns>
		public static bool GetReadOnly(this PropertyInfo propertyInfo)
		{
			var readOnlyAttribute = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>(true);
			return readOnlyAttribute != null && readOnlyAttribute.IsReadOnly;
		}
#endif

		/// <summary>
		///     Retrieve the TypeConverter from the TypeConverterAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <param name="createIfNothingSpecified">true if this should always create a converter</param>
		/// <returns>TypeConverter</returns>
		public static TypeConverter GetTypeConverter(this PropertyInfo propertyInfo, bool createIfNothingSpecified = false)
		{
			var typeConverterAttribute = propertyInfo.GetCustomAttribute<TypeConverterAttribute>(true);
			if (!string.IsNullOrEmpty(typeConverterAttribute?.ConverterTypeName))
			{
				var typeConverterType = Type.GetType(typeConverterAttribute.ConverterTypeName);
				if (typeConverterType != null)
				{
					return (TypeConverter) Activator.CreateInstance(typeConverterType);
				}
			}

			return createIfNothingSpecified ? propertyInfo.PropertyType.GetConverter() : null;
		}
	}
}