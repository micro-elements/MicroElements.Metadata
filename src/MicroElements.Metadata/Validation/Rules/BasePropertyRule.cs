// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Base configurable property validation rule.
    /// Implement <see cref="IsValid"/>.
    /// You can use {propertyName}, {propertyType}, {value} in message format.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public abstract class BasePropertyRule<T> : IValidationRule<T>
    {
        /// <summary>
        /// Gets the property to validate.
        /// </summary>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Checks that property value is valid.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <returns>True if value is valid.</returns>
        protected abstract bool IsValid([AllowNull] T value, IPropertyContainer propertyContainer);

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePropertyRule{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="defaultMessageFormat">Default message format for validation message.</param>
        protected BasePropertyRule(IProperty<T> property, string? defaultMessageFormat = null)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
            this.SetDefaultMessageFormat(defaultMessageFormat);
        }

        /// <inheritdoc/>
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            IPropertyValue<T> propertyValue = propertyContainer.GetPropertyValue(Property, propertyContainer.SearchOptions.ReturnNotDefined())!;

            if (!IsValid(propertyValue.Value, propertyContainer))
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer);
        }
    }
}
