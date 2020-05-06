// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            foreach (var rule in validationRules)
            {
                foreach (Message message in rule.Validate(propertyContainer))
                {
                    yield return message;
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
        /// Returns <see cref="Validation.ValidationResult"/> that holds initial data and validation messages.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validationRules">Validation rules to check.</param>
        /// <returns>Validation result that holds initial data and validation messages.</returns>
        public static ValidationResult ValidationResult(this IPropertyContainer propertyContainer, IEnumerable<IValidationRule> validationRules)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            return new ValidationResult(propertyContainer, propertyContainer.Validate(validationRules));
        }

        /// <summary>
        /// Validates <paramref name="propertyContainer"/> against validation rules from <paramref name="validator"/>.
        /// Returns <see cref="Validation.ValidationResult"/> that holds initial data and validation messages.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <param name="validator">Validator that provides validation rules to check..</param>
        /// <returns>Validation result that holds initial data and validation messages.</returns>
        public static ValidationResult ValidationResult(this IPropertyContainer propertyContainer, IValidator validator)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            validator.AssertArgumentNotNull(nameof(validator));

            return new ValidationResult(propertyContainer, propertyContainer.Validate(validator));
        }
    }
}
