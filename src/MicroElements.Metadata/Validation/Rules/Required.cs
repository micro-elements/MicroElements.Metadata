// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value exists and has not null value.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Required<T> : IPropertyValidationRule<T>
    {
        /// <inheritdoc />
        public IProperty<T> Property { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Required{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public Required(IProperty<T> property)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyValue<T>? propertyValue, IPropertyContainer propertyContainer)
        {
            propertyValue ??= propertyContainer.GetPropertyValue(Property, Search.ExistingOnly);

            if (propertyValue == null)
                yield return this.GetConfiguredMessage(new PropertyValue<T>(Property, default, ValueSource.NotDefined), propertyContainer, "{propertyName} is marked as required but is not exists.");
            else if (propertyValue.Value.IsNull())
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer, "{propertyName} is marked as required but has null value.");
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value exists.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns><see cref="Required{T}"/> validation rule.</returns>
        public static Required<T> Required<T>(this IProperty<T> property)
        {
            return new Required<T>(property);
        }
    }
}
