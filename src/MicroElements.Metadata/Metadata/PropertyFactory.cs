// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

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
}
