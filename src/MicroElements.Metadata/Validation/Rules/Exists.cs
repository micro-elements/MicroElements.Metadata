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
    public class Exists<T> : IValidationRule<T>
    {
        /// <inheritdoc/>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exists{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public Exists(IProperty<T> property)
        {
            Property = property;
            this.SetDefaultMessageFormat("Property {propertyName} is not exists but marked as required");
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            IPropertyValue<T> propertyValue = propertyContainer.GetPropertyValue(Property, useDefaultValue: false);

            if (propertyValue.IsNullOrNotDefined())
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer);
        }
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
