// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Formatters;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    [DebuggerTypeProxy(typeof(PropertyContainerDebugView))]
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
            ParentSource = parentPropertySource;
            if (sourceValues != null)
                _propertyValues.AddRange(sourceValues);
            _searchOptions = searchOptions ?? Search.Default;
        }

        /// <inheritdoc />
        public override string ToString() => Formatter.FullRecursiveFormatter.TryFormat(Properties) ?? PropertyContainer.EmptyName;

        #region IPropertyContainer

        /// <inheritdoc />
        public IPropertyContainer? ParentSource { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => _propertyValues;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _searchOptions;

        #endregion

        #region IReadOnlyCollection

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyValues.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyValues.Count;

        #endregion

        #region Mutability

        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public IPropertyValue<T> SetValue<T>(IProperty<T> property, T? value, ValueSource? valueSource = default)
        {
            property.AssertArgumentNotNull(nameof(property));

            var propertyValue = PropertyValueFactory.Default.Create(property, value, valueSource);
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
        public IPropertyValue<T>? RemoveValue<T>(IProperty<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));

            IPropertyValue? propertyValue = this.GetPropertyValueUntyped(property);

            if (propertyValue != null)
                _propertyValues.Remove(propertyValue);

            return (IPropertyValue<T>?)propertyValue;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _propertyValues.Clear();
        }

        #endregion

        /// <summary>
        /// Gets or calculates value for property.
        /// It's a shortcut for <see cref="SearchExtensions.GetValue{T}"/> for use in derived classes without keyword this.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <param name="defaultValue">Default value that returns if property value was not found.</param>
        /// <returns>The value for property.</returns>
        protected T? GetValue<T>(IProperty<T> property, SearchOptions? search = null, T? defaultValue = default)
        {
            return SearchExtensions.GetValue(this, property, search, defaultValue);
        }
    }
}
