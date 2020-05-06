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
            : base(property)
        {
        }

        /// <inheritdoc />
        protected override bool IsValid(T value, IPropertyContainer propertyContainer)
        {
            return !value.IsNull();
        }

        /// <inheritdoc />
        protected override Message GetMessage(T value, IPropertyContainer propertyContainer)
        {
            return new Message("Property {propertyName} should not be null", severity: MessageSeverity.Error);
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
        /// <returns><see cref="NotNull{T}"/> validation rule.</returns>
        public static NotNull<T> NotNull<T>(this IProperty<T> property)
        {
            return new NotNull<T>(property);
        }
    }
}
