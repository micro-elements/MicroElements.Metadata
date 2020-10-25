// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// ReadOnly property container.
    /// </summary>
    public class PropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Empty property container singleton instance.
        /// </summary>
        public static readonly IPropertyContainer Empty = new PropertyContainer();

        /// <summary>
        /// Real data holder.
        /// </summary>
        private readonly IPropertyContainer _propertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        /// <param name="values">Property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer(
            IEnumerable<IPropertyValue>? values = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            _propertyContainer = new MutablePropertyContainer(values, parentPropertySource, searchOptions);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (ReferenceEquals(this, Empty))
                return "Empty";

            return _propertyContainer.ToString();
        }

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_propertyContainer).GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyContainer.Count;

        /// <inheritdoc />
        public IPropertyValue this[int index] => _propertyContainer[index];

        /// <inheritdoc />
        public IPropertyContainer ParentSource => _propertyContainer.ParentSource;

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => _propertyContainer.Properties;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer.SearchOptions;
    }
}
