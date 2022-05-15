// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata
{
    public static partial class SearchExtensions
    {
        /// <summary>
        /// Gets <see cref="IPropertyValue.ValueUntyped"/> if <paramref name="propertyValue"/> has value.
        /// Returns <see langword="null"/> if <paramref name="propertyValue"/> has no value.
        /// </summary>
        /// <param name="propertyValue">Source PropertyValue.</param>
        /// <returns>Value from <paramref name="propertyValue"/> or <see langword="null"/>.</returns>
        public static object? ValueUntypedOrNull(this IPropertyValue? propertyValue)
        {
            if (propertyValue.HasValue())
                return propertyValue.ValueUntyped;

            return null;
        }

        public static object? ValueUntypedOrDefault(this IPropertyValue propertyValue, object? defaultValue = null)
        {
            if (propertyValue.HasValue())
                return propertyValue.ValueUntyped;

            return defaultValue ?? propertyValue.PropertyUntyped.GetDefaultValueMetadata()?.Value ?? propertyValue.PropertyUntyped.Type.GetDefaultValue();
        }

        public static TResult? MatchValue<T, TResult>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            Func<T?, TResult> hasValue,
            Func<TResult>? noneValue = null,
            SearchOptions? search = null)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, search);

            if (propertyValue.HasValue())
                return hasValue(propertyValue.Value);

            return noneValue != null ? noneValue.Invoke() : default;
        }

        public static TResult? MatchValue<T, TResult>(
            this IPropertyValue<T> propertyValue,
            Func<T?, TResult> hasValue,
            Func<TResult>? noneValue = null)
        {
            hasValue.AssertArgumentNotNull(nameof(hasValue));

            if (propertyValue.HasValue())
                return hasValue(propertyValue.Value);

            return noneValue != null ? noneValue.Invoke() : default;
        }

        public static TResult? MatchValueUntyped<TResult>(
            this IPropertyContainer propertyContainer,
            IProperty property,
            Func<object?, TResult> hasValue,
            Func<TResult> noneValue,
            SearchOptions? search = null)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            IPropertyValue? propertyValue = propertyContainer.GetPropertyValueUntyped(property, search);

            if (propertyValue.HasValue())
                return hasValue(propertyValue.ValueUntyped);

            return noneValue();
        }

        public static TResult MatchValueUntyped<TResult>(
            this IPropertyValue? propertyValue,
            Func<object?, TResult> hasValue,
            Func<TResult> noneValue)
        {
            hasValue.AssertArgumentNotNull(nameof(hasValue));
            noneValue.AssertArgumentNotNull(nameof(noneValue));

            if (propertyValue.HasValue())
                return hasValue(propertyValue.ValueUntyped);

            return noneValue();
        }
    }
}
