// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that target property is exists.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Exists<T> : IPropertyValidationRule<T>
    {
        /// <inheritdoc/>
        public IProperty<T> Property { get; }

        /// <inheritdoc />
        public SearchOptions? SearchOptions => Search.ExistingOnly.ReturnNotDefined();

        /// <summary>
        /// Initializes a new instance of the <see cref="Exists{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public Exists(IProperty<T> property)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
            this.SetDefaultMessageFormat("{propertyName} is not exists.");
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyValue<T>? propertyValue, IPropertyContainer propertyContainer)
        {
            propertyValue ??= propertyContainer.GetPropertyValue(Property, SearchOptions)!;

            if (propertyValue.IsNullOrNotDefined())
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer);
        }

        /// <inheritdoc />
        public override string ToString() => this.GetTypeName();
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that target property is exists.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns><see cref="Exists{T}"/> validation rule.</returns>
        public static Exists<T> Exists<T>(this IProperty<T> property)
        {
            return new Exists<T>(property);
        }
    }
}
