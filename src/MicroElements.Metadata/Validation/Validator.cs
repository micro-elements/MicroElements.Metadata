// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validator that provides cached rules.
    /// </summary>
    public class Validator : IValidator
    {
        private readonly IReadOnlyCollection<IValidationRule> _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="validationRules">Rules to cache.</param>
        public Validator(IEnumerable<IValidationRule> validationRules)
        {
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            _rules = validationRules.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="validationRules">Rules to cache.</param>
        public Validator(params IValidationRule[] validationRules)
        {
            validationRules.AssertArgumentNotNull(nameof(validationRules));

            _rules = validationRules.ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<IValidationRule> GetRules() => _rules;
    }

    /// <summary>
    /// Validator caching extensions.
    /// </summary>
    public static class CachedValidatorExtensions
    {
        /// <summary>
        /// Creates cached validator.
        /// </summary>
        /// <param name="validator">Validator to wrap.</param>
        /// <returns>New validator with cached rules.</returns>
        public static IValidator Cached(this IValidator validator)
        {
            return new Validator(validator.GetRules().ToArray());
        }

        /// <summary>
        /// Creates cached validator from validation rules enumeration.
        /// </summary>
        /// <param name="validationRules">Rules to cache.</param>
        /// <returns>New validator with cached rules.</returns>
        public static IValidator Cached(this IEnumerable<IValidationRule> validationRules)
        {
            return new Validator(validationRules.ToArray());
        }
    }
}
