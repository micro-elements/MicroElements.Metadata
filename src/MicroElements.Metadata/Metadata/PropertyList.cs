// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MicroElements.Functional;
using MicroElements.Utils;

namespace MicroElements.Metadata
{
    /// <summary>
    /// List of properties and values.
    /// </summary>
    public class PropertyList : IReadOnlyList<IPropertyValue>, IPropertyContainer, IMutablePropertyContainer
    {
        private readonly List<IPropertyValue> _propertyValues = new List<IPropertyValue>();
        private readonly IPropertyContainer _parentPropertySource = PropertyContainer.Empty;

        public PropertyList(IEnumerable<IPropertyValue> values = null)
        {
            if (values != null)
                _propertyValues.AddRange(values);
        }

        public PropertyList(IPropertyContainer parentPropertySource, IEnumerable<IPropertyValue> values = null)
        {
            _parentPropertySource = parentPropertySource.AssertArgumentNotNull(nameof(parentPropertySource));
            if (values != null)
                _propertyValues.AddRange(values);
        }

        public PropertyList SetParentPropertySource(IPropertyContainer parentPropertySource)
        {
            return new PropertyList(parentPropertySource, _propertyValues);
        }

        /// <inheritdoc />
        public IPropertyContainer ParentSource => _parentPropertySource;

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => _propertyValues;

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property)
        {
            Type propertyType = property.Type;
            MethodInfo getValue = GetType().GetMethod(nameof(GetValue));
            MethodInfo getValueTyped = getValue.MakeGenericMethod(propertyType);
            return getValueTyped.Invoke(this, new object[] { property });
        }

        /// <inheritdoc />
        public T GetValue<T>(IProperty<T> property)
        {
            // Если свойство в списке свойств, - вернем значение.
            if (_propertyValues.ContainsPropertyByNameOrAlias(property))
            {
                return _propertyValues
                    .FirstOrNone(propertyValue => propertyValue.IsMatchesByNameOrAlias(property))
                    .Map(value => (IPropertyValue<T>)value)
                    .MatchUnsafe(GetPropertyValue, property.DefaultValue);
            }

            // Поищем у родителя.
            if (_parentPropertySource.Properties.ContainsPropertyByNameOrAlias(property))
            {
                return _parentPropertySource.GetValue(property);
            }

            // Свойство не в списке свойств, но может его можно вычислить.
            if (property.Calculate != null)
            {
                return property.Calculate(this);
            }

            // Вернем значение по умолчанию.
            return property.DefaultValue();
        }

        private T GetPropertyValue<T>(IPropertyValue<T> propertyValue)
        {
            if (propertyValue.HasValue())
            {
                return propertyValue.Value;
            }

            return propertyValue.Property.DefaultValue();
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

        public void Add(IPropertyValue propertyValue) => _propertyValues.Add(propertyValue);

        /// <inheritdoc />
        public override string ToString() => _propertyValues.FormatList();

        #region IReadOnlyList<IPropertyValue>

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyValues.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_propertyValues).GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyValues.Count;

        /// <inheritdoc />
        public IPropertyValue this[int index]
        {
            get => _propertyValues[index];
            set => _propertyValues[index] = value;
        }

        #endregion
    }
}
