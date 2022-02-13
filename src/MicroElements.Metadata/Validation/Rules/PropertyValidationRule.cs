// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Generic property validation rule.
    /// Implement <see cref="IsValid"/>.
    /// You can use {propertyName}, {propertyType}, {value} in message format.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public abstract class PropertyValidationRule<T> : IPropertyValidationRule<T>
    {
        /// <summary>
        /// Gets the property to validate.
        /// </summary>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Checks that property value is valid.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <returns>True if value is valid.</returns>
        protected virtual bool IsValid(T? value) => true;

        /// <summary>
        /// Checks that property value is valid.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>True if propertyValue is valid.</returns>
        protected virtual bool IsValidPropertyValue(IPropertyValue<T> propertyValue)
        {
            return IsValid(propertyValue.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationRule{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="defaultMessageFormat">Default message format for validation message.</param>
        protected PropertyValidationRule(IProperty<T> property, string? defaultMessageFormat = null)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
            this.SetDefaultMessageFormat(defaultMessageFormat);
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyValue<T>? propertyValue, IPropertyContainer propertyContainer)
        {
            // Search is from container. ReturnNotDefined is used to assure that propertyValue is always not null.
            // Also default value can be property defined (not always default(T)).
            propertyValue ??= propertyContainer.GetPropertyValue(Property, propertyContainer.SearchOptions.ReturnNotDefined())!;

            if (!IsValidPropertyValue(propertyValue))
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer);
        }

        /// <inheritdoc />
        public override string ToString() => this.GetValidatorName();
    }

    /// <summary>
    /// Non generic property validation rule.
    /// </summary>
    public abstract class PropertyValidationRule : IPropertyValidationRule
    {
        /// <inheritdoc />
        public IProperty PropertyUntyped { get; }

        /// <summary>
        /// Checks that property value is valid.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>True if value is valid.</returns>
        protected abstract bool IsValid(IPropertyValue propertyValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationRule"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="defaultMessageFormat">Default message format for validation message.</param>
        protected PropertyValidationRule(IProperty property, string? defaultMessageFormat = null)
        {
            PropertyUntyped = property.AssertArgumentNotNull(nameof(property));
            this.SetDefaultMessageFormat(defaultMessageFormat);
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyValue? propertyValue, IPropertyContainer propertyContainer)
        {
            // Search is from container. NotDefined uses to assure that propertyValue is always not null.
            // Also default value can be property defined (not always default(T)).
            propertyValue ??= propertyContainer.GetPropertyValueUntyped(PropertyUntyped, propertyContainer.SearchOptions.ReturnNotDefined())!;

            if (propertyValue?.PropertyUntyped != PropertyUntyped)
                yield break;

            if (!IsValid(propertyValue))
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer);
        }
    }
}
