// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value is not default.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class NotDefault<T> : BasePropertyRule<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotDefault{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public NotDefault(IProperty<T> property)
            : base(property, "Property '{propertyName}' should not have default value '{value}'")
        {
        }

        /// <inheritdoc />
        protected override bool IsValid(T value, IPropertyContainer propertyContainer)
        {
            return !value.IsDefault();
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value is not default.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns><see cref="NotDefault{T}"/> validation rule.</returns>
        public static NotDefault<T> NotDefault<T>(this IProperty<T> property)
        {
            return new NotDefault<T>(property);
        }

        /// <summary>
        /// Checks that property value is not default.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <returns>Combined validation rule.</returns>
        public static TValidationRule NotDefault<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker)
            where TValidationRule : IValidationRule<T>
        {
            return linker.CombineWith(linker.FirstRule.Property.NotDefault());
        }
    }
}
