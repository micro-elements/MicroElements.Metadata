﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property extensions.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Gets property name and possible aliases.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Property name aliases.</returns>
        public static IEnumerable<string> GetNameAndAliases(this IProperty property)
        {
            yield return property.Name;
            if (property.GetAlias() is { } alias)
                yield return alias;
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="TResult"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="TSource"/> to type <typeparamref name="TResult"/>.
        /// TSource MayBeNull, TResult MayBeNull but Func at the current moment does not support nullability.</param>
        /// <param name="allowMapNull">By default false: does not calls <paramref name="map"/> if <paramref name="property"/> value is null.</param>
        /// <param name="configureSearch">Allows to reconfigure user search options for current call.</param>
        /// <returns>New property of type <typeparamref name="TResult"/>.</returns>
        public static IProperty<TResult> Map<TSource, TResult>(
            this IProperty<TSource> property,
            Func<TSource, TResult> map,
            bool allowMapNull = false,
            Func<SearchOptions, SearchOptions>? configureSearch = null)
        {
            (TResult, ValueSource) ConvertValue(IPropertyContainer container, SearchOptions search)
            {
                search = configureSearch?.Invoke(search) ?? search;
                IPropertyValue<TSource>? sourcePropertyValue = container.GetPropertyValue(property, search);
                if (sourcePropertyValue.HasValue())
                {
                    TSource sourceValue = sourcePropertyValue.Value;
                    bool shouldMap = !sourceValue.IsNull() || (sourceValue.IsNull() && allowMapNull);
                    if (shouldMap)
                    {
                        // TSource MayBeNull, TResult MayBeNull but Func at the current moment does not support nullability.
                        TResult resultValue = map(sourceValue);
                        return (resultValue, ValueSource.Calculated);
                    }
                }

                return (default(TResult), ValueSource.NotDefined);
            }

            return new Property<TResult>(property.Name)
                .With(description: property.Description, alias: property.Alias)
                .WithCalculate(ConvertValue);
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="TResult"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="TSource"/> to type <typeparamref name="TResult"/>.</param>
        /// <param name="configureSearch">Allows to reconfigure user search options for current call.</param>
        /// <returns>New property of type <typeparamref name="TResult"/>.</returns>
        public static IProperty<TResult> Map<TSource, TResult>(
            this IProperty<TSource> property,
            Func<IPropertyValue<TSource>, (TResult Value, ValueSource ValueSource)> map,
            Func<SearchOptions, SearchOptions>? configureSearch = null)
        {
            (TResult, ValueSource) ConvertValue(IPropertyContainer container, SearchOptions search)
            {
                search = configureSearch?.Invoke(search) ?? search;
                var sourcePropertyValue = container.GetPropertyValue(property, search);
                var resultValue = map(sourcePropertyValue);
                return resultValue;
            }

            return new Property<TResult>(property.Name)
                .With(description: property.Description, alias: property.Alias)
                .WithCalculate(ConvertValue);
        }

        /// <summary>
        /// Converts Nullable struct property to NotNullable property of the same base type.
        /// </summary>
        /// <typeparam name="TSource">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>New property of type <typeparamref name="TSource"/>.</returns>
        public static IProperty<TSource> DeNullify<TSource>(this IProperty<TSource?> property)
            where TSource : struct
        {
            // ReSharper disable once PossibleInvalidOperationException
            return property.Map(map: a => a!.Value, allowMapNull: false);
        }

        /// <summary>
        /// Converts NotNullable struct property to Nullable property of the same base type.
        /// </summary>
        /// <typeparam name="TSource">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>New property of type <typeparamref name="TSource"/>.</returns>
        public static IProperty<TSource?> Nullify<TSource>(this IProperty<TSource> property)
            where TSource : struct
        {
            return property.Map(
                map: a => (TSource?)a,
                allowMapNull: false,
                configureSearch: options => options.UseDefaultValue(false));
        }

        /// <summary>
        /// Calculates undefined struct value to <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="TSource">Value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>New property of type <typeparamref name="TSource"/>.</returns>
        public static IProperty<TSource> UseDefaultForUndefined<TSource>(
            this IProperty<TSource> property,
            TSource defaultValue = default)
            where TSource : struct
        {
            return property.Map(pv => pv.IsNullOrNotDefined() ? (defaultValue, ValueSource.Calculated) : (pv.Value, pv.Source));
        }
    }
}
