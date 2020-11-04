// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public class MutablePropertyContainer : IMutablePropertyContainer
    {
        private readonly List<IPropertyValue> _propertyValues = new List<IPropertyValue>();
        private readonly SearchOptions _searchOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutablePropertyContainer"/> class.
        /// </summary>
        /// <param name="sourceValues">Property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public MutablePropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            ParentSource = parentPropertySource ?? PropertyContainer.Empty;
            if (sourceValues != null)
                _propertyValues.AddRange(sourceValues);
            _searchOptions = searchOptions ?? Search.Default;
        }

        /// <inheritdoc />
        public override string ToString() => _propertyValues.FormatAsTuple(formatValue: StringFormatter.FormatValue);

        #region IPropertyContainer

        /// <inheritdoc />
        public IPropertyContainer ParentSource { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => _propertyValues;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _searchOptions;

        #endregion

        #region IReadOnlyList

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyValues.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_propertyValues).GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyValues.Count;

        /// <inheritdoc />
        public IPropertyValue this[int index] => _propertyValues[index];

        #endregion

        #region Mutability

        /// <summary>
        /// Sets parent property container.
        /// </summary>
        /// <param name="parentPropertySource">Parent property container.</param>
        public void SetParentPropertySource(IPropertyContainer? parentPropertySource)
        {
            ParentSource = parentPropertySource ?? PropertyContainer.Empty;
        }

        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public IPropertyValue<T> SetValue<T>(IProperty<T> property, [AllowNull] T value, ValueSource? valueSource = default)
        {
            property.AssertArgumentNotNull(nameof(property));

            var propertyValue = new PropertyValue<T>(property, value, valueSource);
            SetValue(propertyValue);
            return propertyValue;
        }

        /// <inheritdoc />
        public void SetValue(IPropertyValue propertyValue)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            bool isSet = false;
            for (int i = 0; i < _propertyValues.Count; i++)
            {
                var existingPropertyValue = _propertyValues[i];
                if (_searchOptions.PropertyComparer.Equals(existingPropertyValue.PropertyUntyped, propertyValue.PropertyUntyped))
                {
                    // replaces existing
                    _propertyValues[i] = propertyValue;
                    isSet = true;
                    break;
                }
            }

            if (!isSet)
                _propertyValues.Add(propertyValue);
        }

        /// <inheritdoc />
        public void Add(IPropertyValue propertyValue)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            _propertyValues.Add(propertyValue);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _propertyValues.Clear();
        }

        #endregion
    }
}
