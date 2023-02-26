// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents search algorithm.
    /// </summary>
    public interface ISearchAlgorithm
    {
        /// <summary>
        /// Searches <see cref="IPropertyValue{T}"/> by <see cref="IProperty{T}"/> and <see cref="SearchOptions"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchOptions">Search options.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        IPropertyValue? SearchPropertyValueUntyped(
            IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? searchOptions = null);

        /// <summary>
        /// Gets <see cref="IPropertyValue{T}"/> by <see cref="IProperty{T}"/> and <see cref="SearchOptions"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchOptions">Search options.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        IPropertyValue<T>? GetPropertyValue<T>(
            IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? searchOptions = null);

        void GetPropertyValue2<T>(
            IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? searchOptions,
            out PropertyValueData<T> result);
    }
}
