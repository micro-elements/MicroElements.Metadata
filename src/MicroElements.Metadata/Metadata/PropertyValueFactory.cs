// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class PropertyValueFactory : IPropertyValueFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<IProperty, object, ValueSource, IPropertyValue>> _funcCache = new ();

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
                return (IPropertyValue)new PropertyValue<string>((IProperty<string>)property, (string?)value, source);
            if (propertyType == typeof(IPropertyContainer))
                return new PropertyValue<IPropertyContainer>((IProperty<IPropertyContainer>)property, (IPropertyContainer?)value, source);

            IPropertyValue propertyValue = _funcCache
                .GetOrAdd(propertyType, type => NewPropertyValue(type).Compile())
                .Invoke(property, value!, valueSource!);

            return propertyValue;
        }

        /// <summary>
        /// Returns expression: <code><![CDATA[(property, value, source) => (new PropertyValue<T>((property As IProperty<T>), (value As T), source) As IPropertyValue)]]></code>.
        /// </summary>
        /// <param name="valueType">Value type.</param>
        /// <returns>Expression.</returns>
        public static Expression<Func<IProperty, object, ValueSource, IPropertyValue>> NewPropertyValue(Type valueType)
        {
            Type propertyGenericType = typeof(IProperty<>).MakeGenericType(valueType);
            Type propertyValueGenericType = typeof(PropertyValue<>).MakeGenericType(valueType);

            ParameterExpression untypedPropertyArg = Expression.Parameter(typeof(IProperty), "property");
            ParameterExpression untypedValueArg = Expression.Parameter(typeof(object), "value");
            ParameterExpression sourceArg = Expression.Parameter(typeof(ValueSource), "source");

            var typedPropertyArg = Expression.Convert(untypedPropertyArg, propertyGenericType);
            var typedValueArg = Expression.Convert(untypedValueArg, valueType);

            // ctor: PropertyValue(IProperty<T> property, T value, ValueSource source)
            ConstructorInfo propertyValueConstructor = propertyValueGenericType.GetConstructor(new[] { propertyGenericType, valueType, typeof(ValueSource) })!;

            // new PropertyValue(property as IProperty<T>, value as T, source)
            var newPropertyValue = Expression.New(propertyValueConstructor, typedPropertyArg, typedValueArg, sourceArg);

            // new PropertyValue(property as IProperty<T>, value as T, source) as IPropertyValue
            var castToPropertyValue = Expression.Convert(newPropertyValue, typeof(IPropertyValue));

            // (property, value, source) => (new PropertyValue<T>((property As IProperty<T>), (value As T), source) As IPropertyValue)
            var expression = Expression.Lambda<Func<IProperty, object, ValueSource, IPropertyValue>>(
                castToPropertyValue, untypedPropertyArg, untypedValueArg, sourceArg);

            return expression;
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

        private sealed class PropertyValueKeyComparer : IEqualityComparer<PropertyValueKey>
        {
            private readonly IEqualityComparer<IProperty> _propertyComparer;

            public PropertyValueKeyComparer(IEqualityComparer<IProperty> propertyComparer)
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
            _propertyValuesCache = new ConcurrentDictionary<PropertyValueKey, IPropertyValue>(new PropertyValueKeyComparer(propertyComparer));
        }

        /// <inheritdoc/>
        public IPropertyValue Create(IProperty property, object? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            return _propertyValuesCache.GetOrAdd(new PropertyValueKey(property, value, valueSource), _propertyValueFactory.Create(property, value, valueSource));
        }
    }

    /// <summary>
    /// Extensions for <see cref="IPropertyValueFactory"/>.
    /// </summary>
    public static class PropertyValueFactoryExtensions
    {
        /// <summary>
        /// Creates factory that caches <see cref="IPropertyValue"/> for the same property and value pairs.
        /// </summary>
        /// <param name="propertyValueFactory">Factory.</param>
        /// <param name="propertyComparer">Optional comparer. Default: <see cref="PropertyComparer.ByReferenceComparer"/>.</param>
        /// <returns>New cached <see cref="IPropertyValueFactory"/>.</returns>
        public static IPropertyValueFactory Cached(this IPropertyValueFactory propertyValueFactory, IEqualityComparer<IProperty>? propertyComparer = null)
        {
            propertyValueFactory.AssertArgumentNotNull(nameof(propertyValueFactory));

            return new CachedPropertyValueFactory(propertyValueFactory, propertyComparer ?? PropertyComparer.ByReferenceComparer);
        }
    }
}
