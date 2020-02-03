// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions for <see cref="IPropertyContainer"/>.
    /// </summary>
    public static class PropertyContainerExtensions
    {
        /// <summary>
        /// Returns true if <paramref name="propertyContainer"/> has property.
        /// Also searches in parent sources.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue(this IPropertyContainer propertyContainer, IProperty property, PropertySearch search = PropertySearch.Default)
        {
            return propertyContainer.GetPropertyValueUntyped(property, search) != null;
        }

        public static void SetValueIfNotSet<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value, PropertySearch search = PropertySearch.Equals)
        {
            var propertyValue = propertyContainer.GetPropertyValue(property, search);
            if (propertyValue == null)
                propertyContainer.SetValue(property, value);
        }

        public static IPropertyValue GetPropertyByNameOrAlias(this IEnumerable<IPropertyValue> propertyList, string propertyName)
            => propertyList.FirstOrDefault(propertyValue => IsMatchesByNameOrAlias(propertyValue, propertyName));

        public static IPropertyValue GetProperty(this IReadOnlyList<IPropertyValue> propertyList, IProperty property, PropertySearch propertySearch = PropertySearch.All)
        {
            IPropertyValue result = null;
            if (propertySearch.HasFlag(PropertySearch.Equals))
                result = propertyList.FirstOrDefault(propertyValue => propertyValue.PropertyUntyped == property);
            if (result == null && propertySearch.HasFlag(PropertySearch.ByName))
                result = propertyList.FirstOrDefault(propertyValue => propertyValue.PropertyUntyped.Name.IsOneOf(property.Name, property.Alias, propertySearch));
            if (result == null && propertySearch.HasFlag(PropertySearch.ByAlias))
                result = propertyList.FirstOrDefault(propertyValue => propertyValue.PropertyUntyped.Alias.IsOneOf(property.Name, property.Alias, propertySearch));
            return result;
        }

        public static bool ContainsProperty(this IEnumerable<IPropertyValue> propertyList, IProperty property)
            => propertyList.Any(propertyValue => propertyValue.PropertyUntyped == property);

        public static bool IsMatchesByNameOrAlias(this IPropertyValue propertyValue, string propertyName)
        {
            return propertyName != null
                   && (propertyValue.PropertyUntyped.Name.EqualsIgnoreCase(propertyName)
                       || propertyValue.PropertyUntyped.Alias.EqualsIgnoreCase(propertyName));
        }

        private static bool EqualsIgnoreCase(this string value, string other) =>
            value?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? other == null;

        private static bool IsOneOf(this string value, string other1, string other2, PropertySearch search)
        {
            static StringComparison ComparisonByFlag(PropertySearch propertySearch) =>
                propertySearch.HasFlag(PropertySearch.IgnoreCase)
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            if (value == null)
                return other1 == null || other2 == null;

            return value.Equals(other1, ComparisonByFlag(search)) || value.Equals(other2, ComparisonByFlag(search));
        }
    }
}
