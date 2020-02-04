// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public interface IPropertyContainer : IReadOnlyList<IPropertyValue>, IMetadataProvider
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
        /// Gets the value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>The value for property.</returns>
        T GetValue<T>(IProperty<T> property, bool searchInParent = true);

        /// <summary>
        /// Gets untyped value for property.
        /// </summary>
        /// <param name="property">Property to find.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>The value for property.</returns>
        object GetValueUntyped(IProperty property, bool searchInParent = true);
    }

    /// <summary>
    /// Property container that can mutate it state.
    /// </summary>
    public interface IMutablePropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Sets parent property container.
        /// </summary>
        /// <param name="parentPropertySource">Parent property container.</param>
        void SetParentPropertySource(IPropertyContainer parentPropertySource);

        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        IPropertyValue<T> SetValue<T>(IProperty<T> property, T value);
    }
}
