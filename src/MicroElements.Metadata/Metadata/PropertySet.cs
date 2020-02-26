using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// PropertySet implementation with static property list.
    /// </summary>
    public class PropertySet : IPropertySet
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

        /// <inheritdoc />
        public IEnumerator<IProperty> GetEnumerator() => _properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
