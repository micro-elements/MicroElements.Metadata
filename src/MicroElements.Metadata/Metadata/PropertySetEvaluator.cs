// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Helper class to search <see cref="IPropertySet"/>.
    /// </summary>
    public static class PropertySetEvaluator
    {
        /// <summary>
        /// Gets <see cref="IPropertySet"/> according search conditions.
        /// </summary>
        /// <param name="type">Type that contains PropertySet.</param>
        /// <param name="property">Property with type <see cref="IPropertySet"/> or <see cref="IEnumerable{T}"/>.</param>
        /// <param name="method">Method that returns <see cref="IEnumerable{IProperty}"/>.</param>
        /// <returns><see cref="IPropertySet"/> or null.</returns>
        public static IPropertySet? GetPropertySet(
            Type? type,
            string? property = null,
            string? method = null)
        {
            if (type == null)
                return null;

            IPropertySet? result = null;

            if (type.IsAssignableTo<IPropertySet>() && type.IsConcreteType())
            {
                result = (IPropertySet) Activator.CreateInstance(type);
            }

            bool isStatic = type.IsAbstract && type.IsSealed;

            if (isStatic)
            {
                if (result == null && property != null)
                {
                    PropertyInfo? propertyInfo = type.GetProperty(property, BindingFlags.Static | BindingFlags.Public);
                    if (propertyInfo != null && propertyInfo.PropertyType.IsAssignableTo<IPropertySet>())
                    {
                        result = (IPropertySet)propertyInfo.GetValue(null);
                    }
                }

                if (result == null && method != null)
                {
                    MethodInfo? methodInfo = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public);
                    if (methodInfo != null && methodInfo.ReturnType.IsAssignableTo<IPropertySet>())
                    {
                        result = (IPropertySet)methodInfo.Invoke(null, null);
                    }
                    if (methodInfo != null && methodInfo.ReturnType.IsAssignableTo<IEnumerable<IProperty>>())
                    {
                        IEnumerable<IProperty> properties = (IEnumerable<IProperty>)methodInfo.Invoke(null, null);
                        result = new PropertySet(properties);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets <see cref="IPropertySet"/> from type info.
        /// </summary>
        /// <param name="valueType">Type that implements <see cref="IKnownPropertySet{TPropertySet}"/>.</param>
        /// <returns><see cref="IPropertySet"/> or null.</returns>
        public static IPropertySet? GetSchemaByKnownPropertySet(this Type valueType)
        {
            if (valueType == null)
                return null;

            Type? knownPropertySetType = valueType
                .GetInterfaces()
                .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IKnownPropertySet<>));

            Type? propertySetType = knownPropertySetType
                ?.GenericTypeArguments
                .First();

            return propertySetType != null ? GetPropertySet(type: propertySetType) : null;
        }
    }
}
