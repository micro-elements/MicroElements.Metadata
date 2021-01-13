// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public interface IPropertyContainer : IReadOnlyCollection<IPropertyValue>, IMetadataProvider
    {
        /// <summary>
        /// Gets optional parent property source.
        /// </summary>
        IPropertyContainer? ParentSource { get; }

        /// <summary>
        /// Gets properties with values.
        /// </summary>
        IReadOnlyCollection<IPropertyValue> Properties { get; }

        /// <summary>
        /// Gets default search options for container.
        /// </summary>
        SearchOptions SearchOptions { get; }
    }

    /// <summary>
    /// Property container that can mutate it state.
    /// </summary>
    public interface IMutablePropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        IPropertyValue<T> SetValue<T>(IProperty<T> property, [AllowNull] T value, ValueSource? valueSource = default);

        /// <summary>
        /// Sets property value.
        /// </summary>
        /// <param name="propertyValue">Property and value.</param>
        void SetValue(IPropertyValue propertyValue);

        /// <summary>
        /// Adds new property value.
        /// </summary>
        /// <param name="propertyValue">Property and value.</param>
        void Add(IPropertyValue propertyValue);

        /// <summary>
        /// Removes property value for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that was removed.</returns>
        IPropertyValue<T>? RemoveValue<T>(IProperty<T> property);

        /// <summary>
        /// Clears all property and values.
        /// </summary>
        void Clear();
    }

    public interface IPropertyContainer<TSchema> : IPropertyContainer, IKnownPropertySet<TSchema>, IPropertySet
        where TSchema : IPropertySet, new()
    {
        TSchema Schema { get; }

        /// <inheritdoc />
        IEnumerable<IProperty> IPropertySet.GetProperties() => Schema.GetProperties();
    }
}
