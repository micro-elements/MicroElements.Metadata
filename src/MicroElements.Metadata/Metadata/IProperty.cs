// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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
    public interface IProperty<out T> : IProperty
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        Func<T> DefaultValue { get; }

        /// <summary>
        /// Gets Calculate func for calculated properties.
        /// </summary>
        Func<IPropertyContainer, T> Calculate { get; }

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
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> Map<A, B>(this IProperty<A> property, Func<A, B> map)
        {
            B ConvertValue(IPropertyContainer container)
            {
                A valueA = container.GetValue(property);
                return map(valueA);
            }

            return new Property<B>(property.Name).SetCalculate(ConvertValue);
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func receive value only not null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A"/> to type <typeparamref name="B"/>.</param>
        /// <returns>New property of type <typeparamref name="B"/>.</returns>
        public static IProperty<B> MapNotNull<A, B>(this IProperty<A> property, Func<A, B> map)
            where A : class
            where B : class
        {
            B ConvertNotNullValue(IPropertyContainer container)
            {
                A valueA = container.GetValue(property);
                if (valueA != null)
                    return map(valueA);
                return default;
            }

            return new Property<B>(property.Name).SetCalculate(ConvertNotNullValue);
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func can receive null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A?"/> to type <typeparamref name="B?"/>.</param>
        /// <returns>New property of type <typeparamref name="B?"/>.</returns>
        public static IProperty<B?> Map<A, B>(this IProperty<A?> property, Func<A?, B?> map)
            where A : struct
            where B : struct
        {
            B? ConvertValue(IPropertyContainer container)
            {
                A? valueA = container.GetValue(property);
                return map(valueA);
            }

            return new Property<B?>(property.Name).SetCalculate(ConvertValue);
        }

        /// <summary>
        /// Creates new property of type <typeparamref name="B"/> that evaluates its value as value of property <paramref name="property"/> and <paramref name="map"/> func.
        /// Map func receive value only not null values.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Result type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="map">Function that maps value of type <typeparamref name="A"/> to type <typeparamref name="B"/>.</param>
        /// <returns>New property of type <typeparamref name="B?"/>.</returns>
        public static IProperty<B?> MapNotNull<A, B>(this IProperty<A?> property, Func<A, B> map)
            where A : struct
            where B : struct
        {
            B? ConvertNotNullValue(IPropertyContainer container)
            {
                A? valueA = container.GetValue(property);
                if (valueA.HasValue)
                    return map(valueA.Value);
                return default;
            }

            return new Property<B?>(property.Name).SetCalculate(ConvertNotNullValue);
        }
    }
}
