// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroElements.Metadata
{
    public static class Reflection
    {
        /// <summary>
        /// Autocreates unassigned fields and properties of type <see cref="IProperty{T}"/>.
        /// </summary>
        /// <param name="type">Type that holds static fields and properties.</param>
        /// <param name="prefix">Prefix to add to begin of the property name.</param>
        public static void AutoCreateStaticProperties(this Type type, string prefix = null)
        {
            type
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(fieldInfo => typeof(IProperty).IsAssignableFrom(fieldInfo.FieldType))
                .Where(fieldInfo => fieldInfo.GetValue(null) == null)
                .Foreach(fieldInfo =>
                {
                    Type propertyType = fieldInfo.FieldType.GetGenericArguments().First();
                    string propertyName = $"{prefix}{fieldInfo.Name}";
                    IProperty property = Property.Create(propertyType, propertyName);
                    fieldInfo.SetValue(null, property);
                });

            type
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(propertyInfo => typeof(IProperty).IsAssignableFrom(propertyInfo.PropertyType))
                .Where(propertyInfo => propertyInfo.CanWrite)
                .Where(propertyInfo => propertyInfo.GetValue(null) == null)
                .Foreach(propertyInfo =>
                {
                    Type propertyType = propertyInfo.PropertyType.GetGenericArguments().First();
                    string propertyName = $"{prefix}{propertyInfo.Name}";
                    IProperty property = Property.Create(propertyType, propertyName);
                    propertyInfo.SetValue(null, property);
                });
        }
    }

    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of <see cref="IEnumerable{T}"/>
        /// </summary>
        public static void Foreach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (T item in sequence)
            {
                action(item);
            }
        }
    }
}
