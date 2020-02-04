// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public class MutablePropertyContainer : IMutablePropertyContainer
    {
        private readonly List<IPropertyValue> _propertyValues = new List<IPropertyValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MutablePropertyContainer"/> class.
        /// </summary>
        /// <param name="values">Property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        public MutablePropertyContainer(IEnumerable<IPropertyValue> values = null, IPropertyContainer parentPropertySource = null)
        {
            ParentSource = parentPropertySource ?? PropertyContainer.Empty;
            if (values != null)
                _propertyValues.AddRange(values);
        }

        /// <inheritdoc />
        public override string ToString() => _propertyValues.FormatList();

        #region IPropertyContainer

        /// <inheritdoc />
        public IPropertyContainer ParentSource { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => _propertyValues;

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property, bool searchInParent = true)
        {
            Type propertyType = property.Type;
            MethodInfo getValue = GetType().GetMethod(nameof(GetValue));
            MethodInfo getValueTyped = getValue.MakeGenericMethod(propertyType);
            return getValueTyped.Invoke(this, new object[] { property, searchInParent });
        }

        /// <inheritdoc />
        public T GetValue<T>(IProperty<T> property, bool searchInParent = true) =>
            this.GetPropertyValue(property, searchInParent, calculateValue: true).Value;

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
        public void SetParentPropertySource(IPropertyContainer parentPropertySource)
        {
            ParentSource = parentPropertySource ?? PropertyContainer.Empty;
        }

        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public IPropertyValue<T> SetValue<T>(IProperty<T> property, T value)
        {
            var newPropertyValue = new PropertyValue<T>(property, value, ValueSource.Defined);
            bool isSet = false;
            for (int i = 0; i < _propertyValues.Count; i++)
            {
                var propertyValue = _propertyValues[i];
                if (propertyValue.PropertyUntyped == property)
                {
                    _propertyValues[i] = newPropertyValue;
                    isSet = true;
                    break;
                }
            }

            if (!isSet)
                _propertyValues.Add(newPropertyValue);

            return newPropertyValue;
        }

        #endregion
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static class PropertyContainerBuilder
    {
        /// <summary>
        /// Sets parent property source and returns the same changed propertyContainer.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <returns>The same container with changed parent.</returns>
        public static MutablePropertyContainer WithParentPropertySource(this MutablePropertyContainer propertyContainer, IPropertyContainer parentPropertySource)
        {
            propertyContainer.SetParentPropertySource(parentPropertySource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>The same container with changed property.</returns>
        public static MutablePropertyContainer WithValue<T>(this MutablePropertyContainer propertyContainer, IProperty<T> property, T value)
        {
            propertyContainer.SetValue(property, value);
            return propertyContainer;
        }
    }
}
