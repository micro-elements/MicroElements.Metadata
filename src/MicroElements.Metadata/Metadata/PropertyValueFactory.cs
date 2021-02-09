// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class PropertyValueFactory : IPropertyValueFactory
    {
        /// <inheritdoc/>
        public IPropertyValue Create(IProperty property, object? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            Type propertyType = property.Type;

            if (value != null && !value.IsAssignableTo(propertyType))
                throw new InvalidOperationException($"Value type {value.GetType()} should be the assignable to property type {propertyType}.");

            if (value == null && propertyType.CanNotAcceptNull())
                throw new ArgumentException($"Existing property {property.Name} has type {property.Type} and null value is not allowed");

            ValueSource source = valueSource ?? ValueSource.Defined;

            // Most popular cases:
            if (propertyType == typeof(string))
                return new PropertyValue<string>((IProperty<string>)property, (string?)value, source);
            if (propertyType == typeof(IPropertyContainer))
                return new PropertyValue<IPropertyContainer>((IProperty<IPropertyContainer>)property, (IPropertyContainer?)value, source);

            // Reflection construction. TODO: cache by type
            Type propertyValueType = typeof(PropertyValue<>).MakeGenericType(propertyType);
            IPropertyValue propertyValue = (IPropertyValue)Activator.CreateInstance(propertyValueType, property, value, source);
            return propertyValue;
        }
    }

    /// <summary>
    /// Cached <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class CachedPropertyValueFactory : IPropertyValueFactory
    {
        private readonly struct PropertyValueKey
        {
            public readonly IProperty Property;
            public readonly object? Value;
            public readonly ValueSource? ValueSource;

            public PropertyValueKey(IProperty property, object? value, ValueSource? valueSource)
            {
                Property = property;
                Value = value;
                ValueSource = valueSource;
            }
        }

        private sealed class PropertyValueKeyEqualityComparer : IEqualityComparer<PropertyValueKey>
        {
            private readonly IEqualityComparer<IProperty> _propertyComparer;

            public PropertyValueKeyEqualityComparer(IEqualityComparer<IProperty> propertyComparer)
            {
                _propertyComparer = propertyComparer.AssertArgumentNotNull(nameof(propertyComparer));
            }

            /// <inheritdoc/>
            public bool Equals(PropertyValueKey x, PropertyValueKey y)
            {
                return _propertyComparer.Equals(x.Property, y.Property) && Equals(x.Value, y.Value) && Equals(x.ValueSource, y.ValueSource);
            }

            /// <inheritdoc/>
            public int GetHashCode(PropertyValueKey obj)
            {
                return HashCode.Combine(obj.Property, obj.Value, obj.ValueSource);
            }
        }

        private readonly ConcurrentDictionary<PropertyValueKey, IPropertyValue> _propertyValuesCache;

        private readonly IPropertyValueFactory _propertyValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPropertyValueFactory"/> class.
        /// </summary>
        /// <param name="propertyValueFactory">Real factory.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        public CachedPropertyValueFactory(
            IPropertyValueFactory propertyValueFactory,
            IEqualityComparer<IProperty> propertyComparer)
        {
            propertyValueFactory.AssertArgumentNotNull(nameof(propertyValueFactory));
            propertyComparer.AssertArgumentNotNull(nameof(propertyComparer));

            _propertyValueFactory = propertyValueFactory;
            _propertyValuesCache = new ConcurrentDictionary<PropertyValueKey, IPropertyValue>(new PropertyValueKeyEqualityComparer(propertyComparer));
        }

        /// <inheritdoc/>
        public IPropertyValue Create(IProperty property, object? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            return _propertyValuesCache.GetOrAdd(new PropertyValueKey(property, value, valueSource), _propertyValueFactory.Create(property, value, valueSource));
        }
    }
}
