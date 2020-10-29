// Copyright (c) MicroElements. All rights reserved.
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
            if (property.Alias != null)
                yield return property.Alias;
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A"/> to type <typeparamref name="B"/>.</param>
        /// <param name="allowMapNull">By default false: does not calls <paramref name="map"/> if <paramref name="property"/> value is null.</param>
        /// <param name="configureSearch">Allows to reconfigure user search options for current call.</param>
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> Map<A, B>(
            this IProperty<A> property,
            Func<A, B> map,
            bool allowMapNull = false,
            Func<SearchOptions, SearchOptions>? configureSearch = null)
        {
            (B, ValueSource) ConvertValue(IPropertyContainer container, SearchOptions search)
            {
                search = configureSearch?.Invoke(search) ?? search;
                var propertyValueA = container.GetPropertyValue(property, search);
                if (propertyValueA.HasValue())
                {
                    A valueA = propertyValueA.Value;
                    bool shouldMap = !valueA.IsNull() || (valueA.IsNull() && allowMapNull);
                    if (shouldMap)
                    {
                        B valueB = map(valueA);
                        return (valueB, ValueSource.Calculated);
                    }
                }

                return (default(B), ValueSource.NotDefined);
            }

            return new Property<B>(property.Name)
                .With(description: property.Description, alias: property.Alias)
                .WithCalculate(ConvertValue);
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A"/> to type <typeparamref name="B"/>.</param>
        /// <param name="configureSearch">Allows to reconfigure user search options for current call.</param>
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> Map<A, B>(
            this IProperty<A> property,
            Func<IPropertyValue<A>, (B Value, ValueSource ValueSource)> map,
            Func<SearchOptions, SearchOptions>? configureSearch = null)
        {
            (B, ValueSource) ConvertValue(IPropertyContainer container, SearchOptions search)
            {
                search = configureSearch?.Invoke(search) ?? search;
                var propertyValueA = container.GetPropertyValue(property, search);
                var valueBResult = map(propertyValueA);
                return valueBResult;
            }

            return new Property<B>(property.Name)
                .With(description: property.Description, alias: property.Alias)
                .WithCalculate(ConvertValue);
        }

        /// <summary>
        /// Converts Nullable struct property to NotNullable property of the same base type.
        /// </summary>
        /// <typeparam name="A">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>New property of type <typeparamref name="A"/>.</returns>
        public static IProperty<A> DeNullify<A>(this IProperty<A?> property)
            where A : struct
        {
            // ReSharper disable once PossibleInvalidOperationException
            return property.Map(map: a => a!.Value, allowMapNull: false);
        }

        /// <summary>
        /// Converts NotNullable struct property to Nullable property of the same base type.
        /// </summary>
        /// <typeparam name="A">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>New property of type <typeparamref name="A"/>.</returns>
        public static IProperty<A?> Nullify<A>(this IProperty<A> property)
            where A : struct
        {
            return property.Map(
                map: a => (A?)a,
                allowMapNull: false,
                configureSearch: options => options.UseDefaultValue(false));
        }

        /// <summary>
        /// Calculates undefined struct value to <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="A">Value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>New property of type <typeparamref name="A"/>.</returns>
        public static IProperty<A> UseDefaultForUndefined<A>(
            this IProperty<A> property,
            A defaultValue = default)
            where A : struct
        {
            return property.Map(pv => pv.IsNullOrNotDefined() ? (defaultValue, ValueSource.Calculated) : (pv.Value, pv.Source));
        }
    }
}
