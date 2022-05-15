// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value matches some condition.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class ShouldBe<T> : PropertyValidationRule<T>
    {
        private readonly Func<T?, bool> _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldBe{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="isValid">Function that checks value is valid.</param>
        public ShouldBe(IProperty<T> property, Expression<Func<T?, bool>> isValid)
            : base(property)
        {
            isValid.AssertArgumentNotNull(nameof(isValid));

            _isValid = isValid.Compile();

            string condition = isValid.Body.ToString();
            string valueName = isValid.Parameters[0].Name;

            this.SetDefaultMessageFormat("{propertyName} should match expression: {condition} but {valueName} is '{value}'.");

            this.ConfigureMessage(message => message
                .AddProperty("condition", condition)
                .AddProperty("valueName", valueName));
        }

        /// <inheritdoc />
        protected override bool IsValid(T? value)
        {
            return _isValid(value);
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value matches some condition.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <param name="isValid">Function that checks value is valid.</param>
        /// <returns><see cref="ShouldBe{T}"/> validation rule.</returns>
        public static ShouldBe<T> ShouldBe<T>(this IProperty<T> property, Expression<Func<T?, bool>> isValid)
        {
            isValid.AssertArgumentNotNull(nameof(isValid));

            return new ShouldBe<T>(property, isValid);
        }

        /// <summary>
        /// Checks that property value matches some condition.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <param name="isValid">Function that checks value is valid.</param>
        /// <returns>Combined validation rule.</returns>
        public static TValidationRule ShouldBe<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker, Expression<Func<T?, bool>> isValid)
            where TValidationRule : IPropertyValidationRule<T>
        {
            return linker.CombineWith(linker.FirstRule.Property.ShouldBe(isValid));
        }
    }
}
