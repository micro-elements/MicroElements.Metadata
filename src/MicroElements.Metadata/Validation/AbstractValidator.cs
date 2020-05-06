// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Validation
{
    /// <summary>
    /// AbstractValidator is simple implementation that holds its rules in internal list.
    /// Use <see cref="Add"/> for adding rules.
    /// </summary>
    public abstract class AbstractValidator : IValidator
    {
        private readonly List<IValidationRule> _rules = new List<IValidationRule>();

        /// <inheritdoc />
        public IEnumerable<IValidationRule> GetRules() => _rules;

        /// <summary>
        /// Adds new validation rule.
        /// </summary>
        /// <param name="validationRule">Validation rule to add.</param>
        protected void Add(IValidationRule validationRule)
        {
            _rules.Add(validationRule);
        }
    }
}
