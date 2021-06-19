// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.CodeContracts;
using MicroElements.Functional;

namespace MicroElements.Metadata.Functional
{
    public static class SearchExtensions
    {
        /// <summary>
        /// Gets or calculates optional not null value.
        /// Returns option in <see cref="Some{A}"/> state if property value exists and not null.
        /// Returns None if value was not found.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search options.</param>
        /// <returns>Optional property value.</returns>
        public static Option<T> GetValueAsOption<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? search = null)
        {
            Assertions.AssertArgumentNotNull(propertyContainer, nameof(propertyContainer));
            Assertions.AssertArgumentNotNull(property, nameof(property));

            IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, search);
            if (propertyValue.HasValue() && !propertyValue.Value.IsNull())
            {
                return propertyValue.Value;
            }

            return Option<T>.None;
        }
    }
}
