// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.CodeContracts;
using MicroElements.Metadata;
using MicroElements.Reflection;
using Message = MicroElements.Diagnostics.Message;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value exists and has not null value.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Required<T> : IPropertyValidationRule<T>, IRequiredPropertyValidationRule
    {
        /// <inheritdoc />
        public IProperty<T> Property { get; }

        /// <inheritdoc />
        public SearchOptions? SearchOptions { get; }

        /// <summary>
        /// Gets a value indicating whether the validator should assert if value is null.
        /// </summary>
        public bool AssertValueIsNull { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Required{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="assertValueIsNull">A value indicating whether the validator should assert if value is null.</param>
        /// <param name="searchOptions">Optional search options.</param>
        public Required(IProperty<T> property, bool assertValueIsNull = true, SearchOptions? searchOptions = null)
        {
            AssertValueIsNull = assertValueIsNull;
            property.AssertArgumentNotNull(nameof(property));

            Property = property;
            SearchOptions = searchOptions;
        }

        /// <inheritdoc />
        public IEnumerable<Message> Validate(IPropertyValue<T>? propertyValue, IPropertyContainer propertyContainer)
        {
            propertyValue ??= propertyContainer.GetPropertyValue(Property, SearchOptions ?? propertyContainer.SearchOptions.UseDefaultValue(false).ReturnNotDefined());
            propertyValue ??= new PropertyValue<T>(Property, default, ValueSource.NotDefined);

            if (!propertyValue.HasValue())
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer, "{propertyName} is marked as required but is not exists.");

            if (AssertValueIsNull && propertyValue.HasValue() && propertyValue.Value.IsNull())
                yield return this.GetConfiguredMessage(propertyValue, propertyContainer, "{propertyName} is marked as required but has null value.");
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value exists.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <param name="assertValueIsNull">Also asserts if value is null.</param>
        /// <param name="searchOptions">Optional search options.</param>
        /// <returns><see cref="Required{T}"/> validation rule.</returns>
        public static Required<T> Required<T>(this IProperty<T> property, bool assertValueIsNull = true, SearchOptions? searchOptions = null)
        {
            return new Required<T>(property, assertValueIsNull, searchOptions);
        }
    }
}
