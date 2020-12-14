// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Composite validation rule. Combines two rules.
    /// </summary>
    public class Or : ICompositeValidationRule
    {
        /// <summary>
        /// Gets the first rule.
        /// </summary>
        public IValidationRule FirstRule { get; }

        /// <summary>
        /// Gets the second rule.
        /// </summary>
        public IValidationRule LastRule { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="rule1">First rule.</param>
        /// <param name="rule2">Second rule.</param>
        public Or(IValidationRule rule1, IValidationRule rule2)
        {
            FirstRule = rule1.AssertArgumentNotNull(nameof(rule1));
            LastRule = rule2.AssertArgumentNotNull(nameof(rule2));
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            Message[] messages1 = FirstRule.Validate(propertyContainer).ToArray();
            Message[] messages2 = LastRule.Validate(propertyContainer).ToArray();

            // Rule1 true; Rule2 true => true
            // Rule1 true; Rule2 false => true
            // Rule1 false; Rule2 true => true
            // Rule1 false; Rule2 false => false
            if (messages1.Length > 0 && messages2.Length > 0)
            {
                string message1 = messages1.Select(message => message.FormattedMessage).FormatAsTuple(startSymbol: "[", endSymbol: "]");
                string message2 = messages2.Select(message => message.FormattedMessage).FormatAsTuple(startSymbol: "[", endSymbol: "]");

                yield return new Message($"{message1} or {message2}", severity: MessageSeverity.Error);
            }
        }
    }

    /// <summary>
    /// Composite validation rule. Combines two rules.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Or<T> : IValidationRule<T>, ICompositeValidationRule
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
        /// Initializes a new instance of the <see cref="Or{T}"/> class.
        /// </summary>
        /// <param name="rule1">First rule.</param>
        /// <param name="rule2">Second rule.</param>
        public Or(IValidationRule<T> rule1, IValidationRule<T> rule2)
        {
            FirstRule = rule1.AssertArgumentNotNull(nameof(rule1));
            LastRule = rule2.AssertArgumentNotNull(nameof(rule2));

            Property = rule1.Property;
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            Message[] messages1 = FirstRule.Validate(propertyContainer).ToArray();
            Message[] messages2 = LastRule.Validate(propertyContainer).ToArray();

            // Rule1 true; Rule2 true => true
            // Rule1 true; Rule2 false => true
            // Rule1 false; Rule2 true => true
            // Rule1 false; Rule2 false => false
            if (messages1.Length > 0 && messages2.Length > 0)
            {
                string message1 = messages1.Select(message => message.FormattedMessage).FormatAsTuple(startSymbol: "[", endSymbol: "]");
                string message2 = messages2.Select(message => message.FormattedMessage).FormatAsTuple(startSymbol: "[", endSymbol: "]");

                yield return new Message($"{message1} or {message2}", severity: MessageSeverity.Error);
            }
        }
    }

    /// <summary>
    /// <see cref="Or{T}"/> builder.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class OrBuilder<T> : IValidationRuleLinker<T, Or<T>>
    {
        /// <inheritdoc />
        public IValidationRule<T> FirstRule { get; }

        /// <inheritdoc />
        public Or<T> CombineWith(IValidationRule<T> nextRule)
        {
            nextRule.AssertArgumentNotNull(nameof(nextRule));

            return new Or<T>(FirstRule, nextRule);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrBuilder{T}"/> class.
        /// </summary>
        /// <param name="firstRule">The first rule.</param>
        public OrBuilder(IValidationRule<T> firstRule)
        {
            FirstRule = firstRule.AssertArgumentNotNull(nameof(firstRule));
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Starts to create <see cref="Or{T}"/> composite rule.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="rule1">The first rule.</param>
        /// <returns>Builder.</returns>
        public static OrBuilder<T> Or<T>(this IValidationRule<T> rule1)
        {
            return new OrBuilder<T>(rule1);
        }

        /// <summary>
        /// Creates untyped Or rule.
        /// </summary>
        /// <param name="rule1">The first rule.</param>
        /// <param name="rule2">The second rule.</param>
        /// <returns>Or rule.</returns>
        public static Or Or(this IValidationRule rule1, IValidationRule rule2)
        {
            return new Or(rule1, rule2);
        }
    }
}
