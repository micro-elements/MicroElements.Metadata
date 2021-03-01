// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// ReadOnly property container.
    /// </summary>
    [DebuggerTypeProxy(typeof(PropertyContainerDebugView))]
    public partial class PropertyContainer : IPropertyContainer
    {
        internal const string EmptyName = "Empty";

        /// <summary>
        /// Real data holder.
        /// </summary>
        private readonly IPropertyContainer _propertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        /// <param name="sourceValues">Source property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            _propertyContainer = new MutablePropertyContainer(sourceValues, parentPropertySource, searchOptions);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (ReferenceEquals(this, Empty))
                return EmptyName;

            return _propertyContainer.ToString();
        }

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    /// <summary>
    /// Static PropertyContainer staff.
    /// </summary>
    public partial class PropertyContainer
    {
        /// <summary>
        /// Empty property container singleton instance.
        /// </summary>
        public static readonly IPropertyContainer Empty = new PropertyContainer(searchOptions: SearchOptions.ExistingOnly);

        /// <summary>
        /// Merges <paramref name="propertyContainers"/>.
        /// </summary>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <param name="propertyContainers">Containers to merge with initial.</param>
        /// <returns>New <see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer Merge(PropertyAddMode mergeMode, params IPropertyContainer?[]? propertyContainers)
        {
            return propertyContainers.Merge(mergeMode);
        }
    }
}
