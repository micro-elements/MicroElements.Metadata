// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value is not null.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class NotNull<T> : BasePropertyRule<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNull{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public NotNull(IProperty<T> property)
            : base(property, "{propertyName} should not be null")
        {
        }

        /// <inheritdoc />
        protected override bool IsValid(T value, IPropertyContainer propertyContainer)
        {
            return !value.IsNull();
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value is not null.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns>NotNull validation rule.</returns>
        public static NotNull<T> NotNull<T>(this IProperty<T> property)
            where T : class
        {
            return new NotNull<T>(property);
        }

        /// <summary>
        /// Checks that property value is not null.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns>NotNull validation rule.</returns>
        public static NotNull<T?> NotNull<T>(this IProperty<T?> property)
            where T : struct
        {
            return new NotNull<T?>(property);
        }

        /// <summary>
        /// Checks that property value is not null.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <returns>Combined validation rule.</returns>
        public static TValidationRule NotNull<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker)
            where TValidationRule : IValidationRule<T>
            where T : class
        {
            return linker.CombineWith(new NotNull<T>(linker.FirstRule.Property));
        }

        /// <summary>
        /// Checks that property value is not null.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <returns>Combined validation rule.</returns>
        public static TValidationRule NotNull<T, TValidationRule>(this IValidationRuleLinker<T?, TValidationRule> linker)
            where TValidationRule : IValidationRule<T?>
            where T : struct
        {
            return linker.CombineWith(new NotNull<T?>(linker.FirstRule.Property));
        }
    }
}
