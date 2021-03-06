﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation extensions.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validationRules"/>.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validationRules">Validation rules to check.</param>
        /// <returns>Validation messages.</returns>
        public static IEnumerable<Message> Validate(this IPropertyContainer propertyContainer, IEnumerable<IValidationRule> validationRules)
        {
            if (propertyContainer == null)
                throw new ArgumentNullException(nameof(propertyContainer));
            if (validationRules == null)
                throw new ArgumentNullException(nameof(validationRules));

            foreach (var validationRule in validationRules)
            {
                IEnumerable<Message> validationMessages;
                if (validationRule is IPropertyValidationRule propertyValidationRule)
                    validationMessages = propertyValidationRule.Validate(propertyValue: null, propertyContainer: propertyContainer);
                else
                    validationMessages = validationRule.Validate(propertyContainer);

                foreach (Message validationMessage in validationMessages)
                {
                    yield return validationMessage;
                }
            }
        }

        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validator"/>.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validator">Validator that provides validation rules to check.</param>
        /// <returns>Validation messages.</returns>
        public static IEnumerable<Message> Validate(this IPropertyContainer propertyContainer, IValidator validator)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validator.AssertArgumentNotNull(nameof(validator));

            return propertyContainer.Validate(validator.GetRules());
        }

        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validator"/>.
        /// </summary>
        /// <param name="validator">Validator that provides validation rules to check.</param>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        public static IEnumerable<Message> Validate(this IValidator validator, IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validator.AssertArgumentNotNull(nameof(validator));

            return propertyContainer.Validate(validator.GetRules());
        }

        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validationRules"/>.
        /// Returns <see cref="ValidationResult{T}"/> that holds initial data and validation messages.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validationRules">Validation rules to check.</param>
        /// <returns>Validation result that holds initial data and validation messages.</returns>
        public static ValidationResult<T> ToValidationResult<T>(this T propertyContainer, IEnumerable<IValidationRule> validationRules)
            where T : IPropertyContainer
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            return new ValidationResult<T>(propertyContainer, propertyContainer.Validate(validationRules));
        }

        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validator"/>.
        /// Returns <see cref="ValidationResult{T}"/> that holds initial data and validation messages.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validator">Validator that provides validation rules to check..</param>
        /// <returns>Validation result that holds initial data and validation messages.</returns>
        public static ValidationResult<T> ToValidationResult<T>(this T propertyContainer, IValidator validator)
            where T : IPropertyContainer
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validator.AssertArgumentNotNull(nameof(validator));

            return new ValidationResult<T>(propertyContainer, propertyContainer.Validate(validator));
        }

        /// <summary>
        /// Validates all items with <paramref name="validator"/> and returns all validation messages.
        /// </summary>
        /// <param name="items">Items to validate.</param>
        /// <param name="validator">Validator.</param>
        /// <returns>All validation messages.</returns>
        public static IEnumerable<Message> ValidateAll(this IEnumerable<IPropertyContainer> items, IValidator validator)
        {
            items.AssertArgumentNotNull(nameof(items));
            validator.AssertArgumentNotNull(nameof(validator));

            return items.SelectMany(row => row.Validate(validator));
        }

        /// <summary>
        /// Validates all items against validation rules from <paramref name="validationRules"/>.
        /// </summary>
        /// <typeparam name="TPropertyContainer">Container type.</typeparam>
        /// <param name="items">Items to validate.</param>
        /// <param name="validationRules">Validation rules to check.</param>
        /// <returns>Validation results.</returns>
        public static IEnumerable<ValidationResult<TPropertyContainer>> ToValidationResults<TPropertyContainer>(
            this IEnumerable<TPropertyContainer> items, IEnumerable<IValidationRule> validationRules)
            where TPropertyContainer : IPropertyContainer
        {
            items.AssertArgumentNotNull(nameof(items));
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            return items.Select(row => row.ToValidationResult(validationRules));
        }

        /// <summary>
        /// Validates all items with <paramref name="validator"/> and returns all validation results.
        /// </summary>
        /// <typeparam name="TPropertyContainer">Container type.</typeparam>
        /// <param name="items">Items to validate.</param>
        /// <param name="validator">Validator.</param>
        /// <returns>Validation results.</returns>
        public static IEnumerable<ValidationResult<TPropertyContainer>> ToValidationResults<TPropertyContainer>(
            this IEnumerable<TPropertyContainer> items, IValidator validator)
            where TPropertyContainer : IPropertyContainer
        {
            items.AssertArgumentNotNull(nameof(items));
            validator.AssertArgumentNotNull(nameof(validator));

            return items.Select(row => row.ToValidationResult(validator));
        }

        /// <summary>
        /// Validates and takes only validated values.
        /// Values with validation errors will be skipped and <paramref name="onValidationError"/> call.
        /// </summary>
        /// <typeparam name="TPropertyContainer">Property container type.</typeparam>
        /// <param name="values">Values to validate.</param>
        /// <param name="validator">Validator.</param>
        /// <param name="onValidationError">Optional validation error callback.</param>
        /// <returns>Values without errors.</returns>
        public static IEnumerable<TPropertyContainer> ValidateAndFilter<TPropertyContainer>(
            this IEnumerable<TPropertyContainer> values,
            IValidator validator,
            Action<ValidationResult<TPropertyContainer>> onValidationError = null)
            where TPropertyContainer : IPropertyContainer
        {
            values.AssertArgumentNotNull(nameof(values));
            validator.AssertArgumentNotNull(nameof(validator));

            var validationResults = values.ToValidationResults(validator.Cached());
            foreach (var validationResult in validationResults)
            {
                if (validationResult.IsValid())
                {
                    yield return validationResult.Data;
                }

                onValidationError?.Invoke(validationResult);
            }
        }
    }
}
