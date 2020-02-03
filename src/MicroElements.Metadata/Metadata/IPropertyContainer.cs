// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public interface IPropertyContainer : IReadOnlyList<IPropertyValue>
    {
        /// <summary>
        /// Gets parent property source.
        /// </summary>
        IPropertyContainer ParentSource { get; }

        /// <summary>
        /// Gets properties with values.
        /// </summary>
        IReadOnlyList<IPropertyValue> Properties { get; }

        /// <summary>
        /// Gets property value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="propertySearch">Search conditions.</param>
        /// <returns>Property value or null.</returns>
        IPropertyValue<T> GetPropertyValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default);

        /// <summary>
        /// Gets property value.
        /// </summary>
        /// <param name="property">Property to find.</param>
        /// <param name="propertySearch">Search conditions.</param>
        /// <returns>Property value or null.</returns>
        IPropertyValue GetPropertyValueUntyped(IProperty property, PropertySearch propertySearch = PropertySearch.Default);

        /// <summary>
        /// Gets the value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="propertySearch">Search conditions.</param>
        /// <returns>The value for property.</returns>
        T GetValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default);

        /// <summary>
        /// Gets untyped value for property.
        /// </summary>
        /// <param name="property">Property to find.</param>
        /// <returns>The value for property.</returns>
        object GetValueUntyped(IProperty property);
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
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        IPropertyValue<T> SetValue<T>(IProperty<T> property, T value);
    }

    /// <summary>
    /// Search conditions.
    /// </summary>
    [Flags]
    public enum PropertySearch
    {
        Default = Equals | SearchInParent,
        All = Equals | ByName | ByAlias | IgnoreCase | SearchInParent,

        Equals = 1,
        ByName = 2,
        ByAlias = 4,
        IgnoreCase = 8,
        SearchInParent = 16
    }
}
