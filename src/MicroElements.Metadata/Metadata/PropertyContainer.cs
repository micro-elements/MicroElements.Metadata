using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// ReadOnly property container.
    /// </summary>
    public class PropertyContainer : IPropertyContainer
    {
        private readonly MutablePropertyContainer _propertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        /// <param name="values">Property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        public PropertyContainer(IEnumerable<IPropertyValue> values = null, IPropertyContainer parentPropertySource = null)
        {
            _propertyContainer = new MutablePropertyContainer(values, parentPropertySource);
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
        public IPropertyValue<T> GetPropertyValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default)
            => _propertyContainer.GetPropertyValue(property, propertySearch);

        /// <inheritdoc />
        public IPropertyValue GetPropertyValueUntyped(IProperty property, PropertySearch propertySearch = PropertySearch.Default)
            => _propertyContainer.GetPropertyValueUntyped(property, propertySearch);

        /// <inheritdoc />
        public T GetValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default)
            => _propertyContainer.GetValue(property, propertySearch);

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property) => _propertyContainer.GetValueUntyped(property);
    }
}
