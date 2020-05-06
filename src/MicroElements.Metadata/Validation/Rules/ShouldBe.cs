// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value matches some condition.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class ShouldBe<T> : BasePropertyRule<T>
    {
        private readonly Func<T, bool> _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldBe{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="isValid">Function that checks value is valid.</param>
        public ShouldBe(IProperty<T> property, Func<T, bool> isValid)
            : base(property)
        {
            _isValid = isValid;
        }

        /// <inheritdoc />
        protected override bool IsValid(T value, IPropertyContainer propertyContainer)
        {
            return _isValid(value);
        }

        /// <inheritdoc />
        protected override Message GetMessage(T value, IPropertyContainer propertyContainer)
        {
            return new Message("Property {propertyName} should match condition");
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
        public static ShouldBe<T> ShouldBe<T>(this IProperty<T> property, Func<T, bool> isValid)
        {
            return new ShouldBe<T>(property, isValid);
        }
    }
}
