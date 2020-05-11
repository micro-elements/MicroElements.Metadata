// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Composite validation rule. Combines two rules.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class And<T> : IValidationRule<T>, ICompositeValidationRule
    {
        /// <inheritdoc/>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Gets the first rule.
        /// </summary>
        public IValidationRule FirstRule { get; }

        /// <summary>
        /// Gets the second rule.
        /// </summary>
        public IValidationRule LastRule { get; }

        /// <summary>
        /// Gets a value indicating whether rule should break on first error.
        /// </summary>
        public bool BreakOnFirstError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="And{T}"/> class.
        /// </summary>
        /// <param name="rule1">First rule.</param>
        /// <param name="rule2">Second rule.</param>
        /// <param name="breakOnFirstError">Value indicating whether rule should break on first error.</param>
        public And(IValidationRule<T> rule1, IValidationRule<T> rule2, bool breakOnFirstError = false)
        {
            FirstRule = rule1.AssertArgumentNotNull(nameof(rule1));
            LastRule = rule2.AssertArgumentNotNull(nameof(rule2));

            Property = rule1.Property;
            BreakOnFirstError = breakOnFirstError;
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            foreach (var message in FirstRule.Validate(propertyContainer))
            {
                yield return message;
                if (BreakOnFirstError)
                    yield break;
            }

            foreach (var message in LastRule.Validate(propertyContainer))
            {
                yield return message;
                if (BreakOnFirstError)
                    yield break;
            }
        }
    }

    /// <summary>
    /// <see cref="And{T}"/> builder.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class AndBuilder<T> : IValidationRuleLinker<T, And<T>>
    {
        /// <inheritdoc />
        public IValidationRule<T> FirstRule { get; }

        /// <summary>
        /// Gets a value indicating whether rule should break on first error.
        /// </summary>
        public bool BreakOnFirstError { get; }

        /// <inheritdoc />
        public And<T> CombineWith(IValidationRule<T> nextRule)
        {
            nextRule.AssertArgumentNotNull(nameof(nextRule));

            return new And<T>(FirstRule, nextRule, BreakOnFirstError);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndBuilder{T}"/> class.
        /// </summary>
        /// <param name="firstRule">The first rule.</param>
        /// <param name="breakOnFirstError">Value indicating whether rule should break on first error.</param>
        public AndBuilder(IValidationRule<T> firstRule, bool breakOnFirstError = false)
        {
            FirstRule = firstRule.AssertArgumentNotNull(nameof(firstRule));
            BreakOnFirstError = breakOnFirstError;
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Starts to create <see cref="And{T}"/> composite rule.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="rule1">The first rule.</param>
        /// <param name="breakOnFirstError">Value indicating whether rule should break on first error.</param>
        /// <returns>Builder.</returns>
        public static AndBuilder<T> And<T>(this IValidationRule<T> rule1, bool breakOnFirstError = false)
        {
            return new AndBuilder<T>(rule1, breakOnFirstError);
        }
    }
}
