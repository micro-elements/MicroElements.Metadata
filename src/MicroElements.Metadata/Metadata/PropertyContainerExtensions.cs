// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    internal static class PropertyContainerExtensions
    {
        public static bool HasValue<T>(this IPropertyContainer propertyContainer, IProperty<T> property)
        {
            if (propertyContainer.Properties.HasProperty(property))
                return true;

            if (propertyContainer.ParentSource.Properties.Count > 0 && propertyContainer.ParentSource.HasValue<T>(property))
                return true;

            return false;
        }

        public static bool HasProperty<T>(this IReadOnlyList<IPropertyValue> propertyValues, IProperty<T> property)
        {
            return propertyValues.Any(propertyValue => IsMatchesByCode(propertyValue, property.Code));
        }

        private static bool IsMatchesByCode(IPropertyValue propertyValue, string code)
        {
            return code.Equals(propertyValue.PropertyUntyped.Code, StringComparison.OrdinalIgnoreCase) ||
                   code.Equals(propertyValue.PropertyUntyped.Alias, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or None if no such element is found.
        /// It's like FirstOrDefault but returns Option.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>First element that satisfies <paramref name="predicate"/> or None.</returns>
        public static Option<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
            source.FirstOrDefault(predicate);
    }
}
