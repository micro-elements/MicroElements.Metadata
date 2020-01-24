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
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search property in parent source if not found in container.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue<T>(this IPropertyContainer propertyContainer, IProperty<T> property, bool searchInParent = true)
        {
            if (propertyContainer.Properties.ContainsPropertyByCodeOrAlias(property.Code))
                return true;

            if (searchInParent && propertyContainer.ParentSource.Properties.Count > 0 && propertyContainer.ParentSource.HasValue<T>(property, true))
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="propertyList"/> contains property by name or alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyList">Property list.</param>
        /// <param name="propertyName">Property name to search.</param>
        /// <returns>True if property found in list.</returns>
        public static bool ContainsPropertyByCodeOrAlias(this IReadOnlyList<IPropertyValue> propertyList, string propertyName)
            => propertyList.Any(propertyValue => IsMatchesByCodeOrAlias(propertyValue, propertyName));

        private static bool IsMatchesByCodeOrAlias(this IPropertyValue propertyValue, string propertyName)
        {
            return propertyValue.PropertyUntyped.Code.EqualsIgnoreCase(propertyName) ||
                   propertyValue.PropertyUntyped.Alias.EqualsIgnoreCase(propertyName);
        }

        private static bool EqualsIgnoreCase(this string value, string other) =>
            value?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? other == null;
    }
}
