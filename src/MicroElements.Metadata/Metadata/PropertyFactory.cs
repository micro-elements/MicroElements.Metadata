// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Simple property factory.
    /// </summary>
    public class PropertyFactory : IPropertyFactory
    {
        /// <summary>
        /// Gets default <see cref="IPropertyFactory"/>.
        /// </summary>
        public static IPropertyFactory Default { get; } = new PropertyFactory();

        private static readonly ConcurrentDictionary<Type, Func<IPropertyFactory, string, IProperty>> _funcCache = new ();

        /// <inheritdoc />
        public IProperty<T> Create<T>(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new Property<T>(name);
        }

        /// <inheritdoc />
        public IProperty Create(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // Calls cached compiled expression this.Create<T>(name).
            IProperty property = _funcCache
                .GetOrAdd(type, t => CreateProperty(t).Compile())
                .Invoke(this, name);

            return property;
        }

        /// <summary>
        /// Returns expression: <code><![CDATA[(factory, name) => factory.Create<T>(name) as IProperty]]></code>.
        /// </summary>
        /// <param name="valueType">Value type.</param>
        /// <returns>Expression.</returns>
        private static Expression<Func<IPropertyFactory, string, IProperty>> CreateProperty(Type valueType)
        {
            MethodInfo? createPropertyMethod = typeof(IPropertyFactory).GetMethod(nameof(Create), new [] { typeof(string) });
            if (createPropertyMethod == null)
                throw new InvalidOperationException($"Method {nameof(IPropertyFactory)}.{nameof(Create)} not found!");

            MethodInfo createPropertyGeneric = createPropertyMethod.MakeGenericMethod(valueType);

            // factory.Create<T>(name);
            var factoryParameter = Expression.Parameter(typeof(IPropertyFactory), "factory");
            var nameArg = Expression.Parameter(typeof(string), "name");
            var createProperty = Expression.Call(factoryParameter, createPropertyGeneric, nameArg);

            // factory.Create<T>(name) as IProperty
            var castToProperty = Expression.Convert(createProperty, typeof(IProperty));

            // (factory, name) => factory.Create<T>(name) as IProperty
            var expression = Expression.Lambda<Func<IPropertyFactory, string, IProperty>>(castToProperty, factoryParameter, nameArg);

            return expression;
        }
    }

    /// <summary>
    /// Property factory with predefined properties.
    /// </summary>
    public class PredefinedPropertyFactory : IPropertyFactory, IDecorator<IPropertyFactory>
    {
        private readonly Dictionary<(Type Type, string Name), IProperty> _predefinedProperties;

        /// <inheritdoc />
        public IPropertyFactory Component { get; }

        public PredefinedPropertyFactory(IPropertyFactory component, IEnumerable<IProperty> properties)
        {
            Component = component;
            _predefinedProperties = properties.ToDictionary(property => (property.Type, property.Name), property => property);
        }

        /// <inheritdoc />
        public IProperty<T> Create<T>(string name)
        {
            if (_predefinedProperties.TryGetValue((typeof(T), name), out var property))
                return (IProperty<T>)property;

            return Component.Create<T>(name);
        }

        /// <inheritdoc />
        public IProperty Create(Type type, string name)
        {
            if (_predefinedProperties.TryGetValue((type, name), out var property))
                return property;

            return Component.Create(type, name);
        }
    }

    public static partial class PropertyFactoryExtensions
    {
        public static IPropertyFactory WithPredefinedProperties(this IPropertyFactory propertyFactory, IEnumerable<IProperty> properties)
        {
            return new PredefinedPropertyFactory(propertyFactory, properties);
        }

        public static IPropertyFactory WithPredefinedProperties(this IPropertyFactory propertyFactory, IPropertySet propertySet)
        {
            return new PredefinedPropertyFactory(propertyFactory, propertySet.GetProperties());
        }
    }
}
