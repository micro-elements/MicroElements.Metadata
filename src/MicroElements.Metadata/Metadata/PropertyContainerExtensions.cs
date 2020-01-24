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
        /// <param name="searchInParent">Search property in parent source if not found in container.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue(this IPropertyContainer propertyContainer, IProperty property, bool searchInParent = true)
        {
            if (propertyContainer.Properties.ContainsProperty(property))
                return true;

            if (searchInParent && propertyContainer.ParentSource.Properties.Count > 0 && propertyContainer.ParentSource.HasValue(property, true))
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="propertyList"/> contains property by name or alias.
        /// </summary>
        /// <param name="propertyList">Property list.</param>
        /// <param name="propertyName">Property name to search.</param>
        /// <returns>True if property found in list.</returns>
        public static bool ContainsPropertyByCodeOrAlias(this IEnumerable<IPropertyValue> propertyList, string propertyName)
            => propertyList.Any(propertyValue => IsMatchesByCodeOrAlias(propertyValue, propertyName));

        public static bool ContainsPropertyByCodeOrAlias(this IEnumerable<IPropertyValue> propertyList, IProperty property)
            => propertyList.Any(propertyValue => IsMatchesByCodeOrAlias(propertyValue, property));

        public static bool ContainsProperty(this IEnumerable<IPropertyValue> propertyList, IProperty property)
            => propertyList.Any(propertyValue => propertyValue.PropertyUntyped == property);

        public static bool IsMatchesByCodeOrAlias(this IPropertyValue propertyValue, IProperty property)
        {
            return propertyValue.IsMatchesByCodeOrAlias(property.Code) ||
                   propertyValue.IsMatchesByCodeOrAlias(property.Alias);
        }

        public static bool IsMatchesByCodeOrAlias(this IPropertyValue propertyValue, string propertyName)
        {
            return propertyValue.PropertyUntyped.Code.EqualsIgnoreCase(propertyName) ||
                   propertyValue.PropertyUntyped.Alias.EqualsIgnoreCase(propertyName);
        }

        private static bool EqualsIgnoreCase(this string value, string other) =>
            value?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? other == null;
    }
}
