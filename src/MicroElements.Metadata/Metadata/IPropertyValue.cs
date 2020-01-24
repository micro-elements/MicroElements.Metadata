// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property and its value.
    /// </summary>
    public interface IPropertyValue
    {
        /// <summary>
        /// Gets property.
        /// </summary>
        IProperty PropertyUntyped { get; }

        /// <summary>
        /// Gets property value.
        /// </summary>
        object ValueUntyped { get; }

        /// <summary>
        /// Gets value source.
        /// </summary>
        ValueSource Source { get; }
    }

    /// <summary>
    /// Strong typed property and value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyValue<out T> : IPropertyValue
    {
        /// <summary>
        /// Gets property.
        /// </summary>
        IProperty<T> Property { get; }

        /// <summary>
        /// Gets property value.
        /// </summary>
        T Value { get; }
    }

    public static class PropertyValueExtensions
    {
        /// <summary>
        /// Returns true if property has value. (Not calculated value but set).
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyValue">PropertyValue instance.</param>
        /// <returns>true if property has value.</returns>
        public static bool HasValue<T>(this IPropertyValue<T> propertyValue) => !propertyValue.Value.IsDefault() || propertyValue.Source != ValueSource.NotDefined;
    }
}
