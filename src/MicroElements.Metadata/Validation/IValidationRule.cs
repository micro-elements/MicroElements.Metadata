// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Serialization;
using MicroElements.Reflection.FriendlyName;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation rule.
    /// Validates <see cref="IPropertyContainer"/>.
    /// Returns validation messages.
    /// </summary>
    public interface IValidationRule : IMetadataProvider
    {
        /// <summary>
        /// Validates property value or other aspect of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        IEnumerable<Message> Validate(IPropertyContainer propertyContainer);
    }

    /// <summary>
    /// Validation rule for single property.
    /// </summary>
    public interface IPropertyValidationRule : IValidationRule
    {
        /// <summary>
        /// Gets property to validate.
        /// </summary>
        IProperty PropertyUntyped { get; }

        /// <summary>
        /// Gets default search options for property search.
        /// </summary>
        SearchOptions? SearchOptions => null;

        /// <summary>
        /// Validates property value or other aspect of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyValue">Optional PropertyValue to validate. If not set then it will be get from source <paramref name="propertyContainer"/>.</param>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        IEnumerable<Message> Validate(IPropertyValue? propertyValue, IPropertyContainer propertyContainer);

        #region AutoImplemented base interfaces

        /// <inheritdoc />
        IEnumerable<Message> IValidationRule.Validate(IPropertyContainer propertyContainer)
        {
            // Search is from container. NotDefined uses to assure that propertyValue is always not null.
            // Also default value can be property defined (not always default(T)).
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(PropertyUntyped, SearchOptions ?? propertyContainer.SearchOptions.ReturnNotDefined())!;

            // Use validate with propertyValue.
            return Validate(propertyValue, propertyContainer);
        }

        #endregion
    }

    public static class ValidationRuleExtensions
    {
        public static IPropertyValidationRule<T> As<T>(this IValidationRule validationRule) => (IPropertyValidationRule<T>)validationRule;

        public static string GetValidatorName(this IPropertyValidationRule validationRule) => $"{validationRule.GetType().GetFriendlyName()}({validationRule.PropertyUntyped})";
    }

    /// <summary>
    /// Typed property validation rule.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyValidationRule<T> : IPropertyValidationRule
    {
        /// <summary>
        /// Gets property to validate.
        /// </summary>
        IProperty<T> Property { get; }

        /// <summary>
        /// Validates property or other aspect of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyValue">Optional propertyValue to validate.</param>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        IEnumerable<Message> Validate(IPropertyValue<T>? propertyValue, IPropertyContainer propertyContainer);

        #region AutoImplemented base interfaces

        /// <inheritdoc />
        IProperty IPropertyValidationRule.PropertyUntyped => Property;

        /// <inheritdoc />
        IEnumerable<Message> IValidationRule.Validate(IPropertyContainer propertyContainer)
        {
            // If SearchOptions is not defined then it will be get from container. NotDefined used to assure that propertyValue is always not null (its needed for proper message generation).
            // Also default value can be property defined (not always default(T)).
            IPropertyValue propertyValue = propertyContainer.GetPropertyValue(Property, SearchOptions ?? propertyContainer.SearchOptions.ReturnNotDefined())!;

            // Use strong typed implementation.
            return Validate((IPropertyValue<T>?)propertyValue, propertyContainer);
        }

        /// <inheritdoc />
        IEnumerable<Message> IPropertyValidationRule.Validate(IPropertyValue? propertyValue, IPropertyContainer propertyContainer)
        {
            // Use strong typed implementation.
            return Validate((IPropertyValue<T>?)propertyValue, propertyContainer);
        }

        #endregion
    }

    /// <summary>
    /// Composite validation rule.
    /// </summary>
    public interface ICompositeValidationRule : IValidationRule
    {
        /// <summary>
        /// Gets the first rule.
        /// </summary>
        IValidationRule FirstRule { get; }

        /// <summary>
        /// Gets the last rule.
        /// </summary>
        IValidationRule LastRule { get; }
    }
}
