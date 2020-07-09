// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents metadata for property.
    /// Every object consist of many properties and we should map properties from different sources to one common model.
    /// </summary>
    public interface IProperty : IMetadataProvider, IEnumerable<IProperty>
    {
        /// <summary>
        /// Unique property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Property value type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Property description.
        /// </summary>
        LocalizableString Description { get; }

        /// <summary>
        /// Alternative code for property.
        /// </summary>
        string Alias { get; }
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IProperty<T> : IProperty
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        Func<T> DefaultValue { get; }

        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        IPropertyCalculator<T> Calculator { get; }

        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyList<T> Examples { get; }
    }

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
        /// <param name="searchOptions">Search options to get property value.</param>
        /// <param name="allowMapNull">By default false: does not calls <paramref name="map"/> if <paramref name="property"/> value is null.</param>
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> Map<A, B>(
            this IProperty<A> property,
            Func<A, B> map,
            SearchOptions searchOptions = default,
            bool allowMapNull = false)
        {
            (B, ValueSource) ConvertValue(IPropertyContainer container)
            {
                var propertyValueA = container.GetPropertyValue(property, searchOptions);
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

                return (default, ValueSource.NotDefined);
            }

            return new Property<B>(property.Name).SetCalculate(container => ConvertValue(container));
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A"/> to type <typeparamref name="B"/>.</param>
        /// <param name="searchOptions">Search options to get property value.</param>
        /// <param name="allowMapNull">By default false: does not calls <paramref name="map"/> if <paramref name="property"/> value is null.</param>
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> Map<A, B>(
            this IProperty<A> property,
            Func<A, (B Value, ValueSource ValueSource)> map,
            SearchOptions searchOptions = default,
            bool allowMapNull = false)
        {
            (B, ValueSource) ConvertValue(IPropertyContainer container)
            {
                var propertyValueA = container.GetPropertyValue(property, searchOptions);
                if (propertyValueA.HasValue())
                {
                    A valueA = propertyValueA.Value;
                    bool shouldMap = !valueA.IsNull() || (valueA.IsNull() && allowMapNull);
                    if (shouldMap)
                    {
                        var valueBResult = map(valueA);
                        return valueBResult;
                    }
                }

                return (default, ValueSource.NotDefined);
            }

            return new Property<B>(property.Name).SetCalculate(container => ConvertValue(container));
        }

        /// <summary>
        /// Converts Nullable property to NotNullable property of the same base type.
        /// </summary>
        /// <typeparam name="A">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="searchOptions">Search options to get property value.</param>
        /// <returns>New property of type <typeparamref name="A"/>.</returns>
        public static IProperty<A> DeNull<A>(this IProperty<A?> property, SearchOptions searchOptions = default)
            where A : struct
        {
            return Map(property, a => a.Value, allowMapNull: false, searchOptions: searchOptions);
        }
    }
}
