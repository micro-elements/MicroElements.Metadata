// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents <see cref="IPropertyContainer"/> with schema.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    public class PropertyContainer<TSchema> : PropertyContainer, IKnownPropertySet<TSchema>
        where TSchema : IPropertySet, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer{TSchema}"/> class.
        /// </summary>
        /// <param name="sourceValues">Source property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
            : base(sourceValues, parentPropertySource, searchOptions)
        {
        }
    }

    public class PropertyContainer2<TSchema> : PropertyContainerBase, IKnownPropertySet<TSchema>
        where TSchema : IPropertySet, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer{TSchema}"/> class.
        /// </summary>
        /// <param name="sourceValues">Source property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer2(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
            : base(PropertyContainer.Empty)
        {
            _propertyContainer = new PropertyContainer(sourceValues, parentPropertySource, searchOptions);
        }

        /// <inheritdoc />
        public PropertyContainer2(IPropertyContainer propertyContainer)
            : base(propertyContainer)
        {
        }
    }

    public class PropertyContainerBase : IPropertyContainer
    {
        protected IPropertyContainer _propertyContainer;

        public PropertyContainerBase(IPropertyContainer propertyContainer)
        {
            _propertyContainer = propertyContainer;
        }

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator()
        {
            return _propertyContainer.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_propertyContainer).GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _propertyContainer.Count;

        /// <inheritdoc />
        public IPropertyContainer? ParentSource => _propertyContainer.ParentSource;

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer.Properties;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer.SearchOptions;

        /// <summary>
        /// Gets or calculates value for property.
        /// For use in derived classes.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <returns>The value for property.</returns>
        [return: MaybeNull]
        protected T GetValue<T>(IProperty<T> property, SearchOptions? search = null)
        {
            return SearchExtensions.GetValue(this, property, search);
        }
    }
}
