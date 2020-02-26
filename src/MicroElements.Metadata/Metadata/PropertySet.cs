// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// PropertySet implementation with cached property list.
    /// </summary>
    public sealed class PropertySet : IPropertySet
    {
        private readonly List<IProperty> _properties = new List<IProperty>();

        /// <summary>
        /// Gets the property list.
        /// </summary>
        public IReadOnlyList<IProperty> Properties => _properties;

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties() => _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySet"/> class.
        /// </summary>
        /// <param name="properties">Property list.</param>
        public PropertySet(params IProperty[] properties)
        {
            if (properties != null && properties.Length > 0)
                _properties.AddRange(properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySet"/> class.
        /// </summary>
        /// <param name="properties">Property enumeration.</param>
        public PropertySet(IEnumerable<IProperty> properties)
        {
            if (properties != null)
                _properties.AddRange(properties);
        }

        /// <inheritdoc />
        public IEnumerator<IProperty> GetEnumerator() => _properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// PropertySet implementation with enumerable from constructor.
    /// </summary>
    public sealed class PropertySetEnumerable : IPropertySet
    {
        private readonly IEnumerable<IProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetEnumerable"/> class.
        /// </summary>
        /// <param name="properties">Property enumeration.</param>
        public PropertySetEnumerable(IEnumerable<IProperty> properties)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties() => _properties;

        /// <inheritdoc />
        public IEnumerator<IProperty> GetEnumerator() => GetProperties().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// PropertySet implementation with enumerable from inheritor.
    /// </summary>
    public abstract class PropertySetBase : IPropertySet
    {
        /// <inheritdoc />
        public abstract IEnumerable<IProperty> GetProperties();

        /// <inheritdoc />
        public IEnumerator<IProperty> GetEnumerator() => GetProperties().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
