// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.Collections.TwoLayerCache;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class PropertyValueFactory : IPropertyValueFactory
    {
        private readonly bool _assumeNullForNotNullableAsNotDefined;

        /// <summary>
        /// Gets default <see cref="IPropertyValue"/> factory instance.
        /// </summary>
        public static IPropertyValueFactory Default { get; } = new PropertyValueFactory();

        private static readonly ConcurrentDictionary<Type, Func<IPropertyValueFactory, IProperty, object, ValueSource, IPropertyValue>> _funcCache = new ();

        public PropertyValueFactory(bool assumeNullForNotNullableAsNotDefined = true)
        {
            _assumeNullForNotNullableAsNotDefined = assumeNullForNotNullableAsNotDefined;
        }

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
            {
                if (_assumeNullForNotNullableAsNotDefined)
                {
                    value = TypeExtensions.GetDefaultValue(propertyType);
                    valueSource = ValueSource.NotDefined;
                }
                else
                {
                    throw new ArgumentException($"Existing property {property.Name} has type {property.Type} and null value is not allowed");
                }
            }

            // Most popular cases:
            if (propertyType == typeof(string))
                return Create((IProperty<string>)property, (string?)value, valueSource);

            if (propertyType == typeof(IPropertyContainer))
                return Create((IProperty<IPropertyContainer>)property, (IPropertyContainer?)value, valueSource);

            // Calls this.Create<T>(property, value, valueSource)
            IPropertyValue propertyValue = _funcCache
                .GetOrAdd(propertyType, type => CreatePropertyValue(type).Compile())
                .Invoke(this, property, value!, valueSource!);

            return propertyValue;
        }

        /// <summary>
        /// Returns expression: <code><![CDATA[(factory, property, value, source) => (new PropertyValue<T>(factory.Create<T>(property as IProperty<T>, value as T, source) as IPropertyValue)]]></code>.
        /// </summary>
        /// <param name="valueType">Value type.</param>
        /// <returns>Expression.</returns>
        public static Expression<Func<IPropertyValueFactory, IProperty, object, ValueSource, IPropertyValue>> CreatePropertyValue(Type valueType)
        {
            // IPropertyValue<T> Create<T>(IProperty<T> property, T? value, ValueSource? valueSource = null)
            MethodInfo? createPropertyMethod = typeof(IPropertyValueFactory).GetMethod(nameof(Create));
            if (createPropertyMethod == null)
                throw new InvalidOperationException($"Method {nameof(IPropertyValueFactory)}.{nameof(Create)} not found!");
            MethodInfo createPropertyGeneric = createPropertyMethod.MakeGenericMethod(valueType);

            ParameterExpression factoryParameter = Expression.Parameter(typeof(IPropertyValueFactory), "factory");
            ParameterExpression untypedPropertyArg = Expression.Parameter(typeof(IProperty), "property");
            ParameterExpression untypedValueArg = Expression.Parameter(typeof(object), "value");
            ParameterExpression sourceArg = Expression.Parameter(typeof(ValueSource), "source");

            // property as IProperty<T>
            var typedPropertyArg = Expression.Convert(untypedPropertyArg, typeof(IProperty<>).MakeGenericType(valueType));

            // value as T
            var typedValueArg = Expression.Convert(untypedValueArg, valueType);

            // factory.Create<T>(property as IProperty<T>, value as T, source)
            var newPropertyValue = Expression.Call(factoryParameter, createPropertyGeneric, typedPropertyArg, typedValueArg, sourceArg);

            // factory.Create<T>(property as IProperty<T>, value as T, source) as IPropertyValue
            var castToPropertyValue = Expression.Convert(newPropertyValue, typeof(IPropertyValue));

            // (factory, property, value, source) => (new PropertyValue<T>(factory.Create<T>(property as IProperty<T>, value as T, source) as IPropertyValue)
            var expression = Expression.Lambda<Func<IPropertyValueFactory, IProperty, object, ValueSource, IPropertyValue>>(
                castToPropertyValue, factoryParameter, untypedPropertyArg, untypedValueArg, sourceArg);

            return expression;
        }
    }

    /// <summary>
    /// Cached <see cref="IPropertyValue"/> factory.
    /// </summary>
    public class CachedPropertyValueFactory : IPropertyValueFactory
    {
        private readonly struct PropertyValueInfo : IEquatable<PropertyValueInfo>
        {
            public readonly IProperty Property;
            public readonly object? Value;
            public readonly ValueSource? ValueSource;
            public readonly IPropertyValueFactory PropertyValueFactory;
            public readonly IEqualityComparer<IProperty> PropertyComparer;

            public PropertyValueInfo(
                IProperty property,
                object? value,
                ValueSource? valueSource,
                IPropertyValueFactory propertyValueFactory,
                IEqualityComparer<IProperty> propertyComparer)
            {
                Property = property;
                Value = value;
                ValueSource = valueSource;
                PropertyValueFactory = propertyValueFactory;
                PropertyComparer = propertyComparer;
            }

            /// <inheritdoc />
            public bool Equals(PropertyValueInfo other)
            {
                return PropertyComparer.Equals(Property, other.Property) && Equals(Value, other.Value) && Equals(ValueSource, other.ValueSource);
            }

            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is PropertyValueInfo other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Property, Value, ValueSource, PropertyValueFactory);
        }

        private readonly TwoLayerCache<PropertyValueInfo, IPropertyValue> _propertyValuesCache;
        private readonly IPropertyValueFactory _propertyValueFactory;
        private readonly IEqualityComparer<IProperty> _propertyComparer;

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
            _propertyComparer = propertyComparer;
            _propertyValuesCache = new TwoLayerCache<PropertyValueInfo, IPropertyValue>(maxItemCount.GetValueOrDefault(4000));
        }

        /// <inheritdoc />
        public IPropertyValue<T> Create<T>(IProperty<T> property, T? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            var cacheKey = new PropertyValueInfo(property, value, valueSource, _propertyValueFactory, _propertyComparer);
            return (IPropertyValue<T>)_propertyValuesCache.GetOrAdd(cacheKey, propertyValueInfo => CreatePropertyValue<T>(propertyValueInfo));
        }

        /// <inheritdoc/>
        public IPropertyValue CreateUntyped(IProperty property, object? value, ValueSource? valueSource = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            var cacheKey = new PropertyValueInfo(property, value, valueSource, _propertyValueFactory, _propertyComparer);
            return _propertyValuesCache.GetOrAdd(cacheKey, propertyValueInfo => CreatePropertyValueUntyped(propertyValueInfo));
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
