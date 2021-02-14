// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Core;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class PropertyValueFactory : IPropertyValueFactory
    {
        /// <summary>
        /// Gets default <see cref="IPropertyValue"/> factory instance.
        /// </summary>
        public static IPropertyValueFactory Default { get; } = new PropertyValueFactory();

        private static readonly ConcurrentDictionary<Type, Func<IProperty, object, ValueSource, IPropertyValue>> _funcCache = new ();

        /// <inheritdoc />
        public IPropertyValue<T> Create<T>(IProperty<T> property, T? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            return new PropertyValue<T>(property, value, valueSource);
        }

        /// <inheritdoc/>
        public IPropertyValue CreateUntyped(IProperty property, object? value, ValueSource? valueSource = null)
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
        private readonly struct PropertyValueInfo
        {
            public readonly IProperty Property;
            public readonly object? Value;
            public readonly ValueSource? ValueSource;
            public readonly IPropertyValueFactory PropertyValueFactory;

            public PropertyValueInfo(IProperty property, object? value, ValueSource? valueSource, IPropertyValueFactory propertyValueFactory)
            {
                Property = property;
                Value = value;
                ValueSource = valueSource;
                PropertyValueFactory = propertyValueFactory;
            }
        }

        private sealed class PropertyValueKeyComparer : IEqualityComparer<PropertyValueInfo>
        {
            private readonly IEqualityComparer<IProperty> _propertyComparer;

            public PropertyValueKeyComparer(IEqualityComparer<IProperty> propertyComparer)
            {
                _propertyComparer = propertyComparer.AssertArgumentNotNull(nameof(propertyComparer));
            }

            /// <inheritdoc/>
            public bool Equals(PropertyValueInfo x, PropertyValueInfo y)
            {
                return _propertyComparer.Equals(x.Property, y.Property) && Equals(x.Value, y.Value) && Equals(x.ValueSource, y.ValueSource);
            }

            /// <inheritdoc/>
            public int GetHashCode(PropertyValueInfo obj)
            {
                return HashCode.Combine(obj.Property, obj.Value, obj.ValueSource);
            }
        }

        private readonly ICache<PropertyValueInfo, IPropertyValue> _propertyValuesCache;

        private readonly IPropertyValueFactory _propertyValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPropertyValueFactory"/> class.
        /// </summary>
        /// <param name="propertyValueFactory">Real factory.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <param name="maxItemCount">Max item count in cache.</param>
        public CachedPropertyValueFactory(
            IPropertyValueFactory propertyValueFactory,
            IEqualityComparer<IProperty> propertyComparer,
            int? maxItemCount = null)
        {
            propertyValueFactory.AssertArgumentNotNull(nameof(propertyValueFactory));
            propertyComparer.AssertArgumentNotNull(nameof(propertyComparer));

            _propertyValueFactory = propertyValueFactory;

            var propertyValueKeyComparer = new PropertyValueKeyComparer(propertyComparer);

            if (maxItemCount == null)
            {
                // unlimited cache
                _propertyValuesCache = new ConcurrentDictionaryAdapter<PropertyValueInfo, IPropertyValue>(new ConcurrentDictionary<PropertyValueInfo, IPropertyValue>(propertyValueKeyComparer));
            }
            else
            {
                // limited cache
                _propertyValuesCache = new Core.TwoLayerCache<PropertyValueInfo, IPropertyValue>(maxItemCount.Value, propertyValueKeyComparer);
            }
        }

        /// <inheritdoc />
        public IPropertyValue<T> Create<T>(IProperty<T> property, T? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            return (IPropertyValue<T>)_propertyValuesCache.GetOrAdd(new PropertyValueInfo(property, value, valueSource, _propertyValueFactory), propertyValueInfo => CreatePropertyValue<T>(propertyValueInfo));
        }

        /// <inheritdoc/>
        public IPropertyValue CreateUntyped(IProperty property, object? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            return _propertyValuesCache.GetOrAdd(new PropertyValueInfo(property, value, valueSource, _propertyValueFactory), propertyValueInfo => CreatePropertyValueUntyped(propertyValueInfo));
        }

        private static IPropertyValue<T> CreatePropertyValue<T>(in PropertyValueInfo info)
        {
            return info.PropertyValueFactory.Create((IProperty<T>)info.Property, (T)info.Value, info.ValueSource);
        }

        private static IPropertyValue CreatePropertyValueUntyped(in PropertyValueInfo info)
        {
            return info.PropertyValueFactory.CreateUntyped(info.Property, info.Value, info.ValueSource);
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
        /// <param name="maxItemCount">Max item count in cache.</param>
        /// <returns>New cached <see cref="IPropertyValueFactory"/>.</returns>
        public static IPropertyValueFactory Cached(this IPropertyValueFactory propertyValueFactory, IEqualityComparer<IProperty>? propertyComparer = null, int? maxItemCount = null)
        {
            propertyValueFactory.AssertArgumentNotNull(nameof(propertyValueFactory));

            return new CachedPropertyValueFactory(
                propertyValueFactory,
                propertyComparer ?? PropertyComparer.ByReferenceComparer,
                maxItemCount: maxItemCount);
        }
    }
}
