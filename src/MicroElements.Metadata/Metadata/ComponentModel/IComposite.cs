// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.Collections.TwoLayerCache;
using MicroElements.Reflection.CodeCompiler;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Represents an object that contains from other components and provides that components in runtime.
    /// </summary>
    public interface IComposite
    {
        /// <summary>
        /// Enumerates not null components of the composite.
        /// Default implementation gets components from implemented <see cref="IHas{T}"/> interfaces.
        /// </summary>
        /// <returns>Enumeration of components.</returns>
        IEnumerable<object> Components()
        {
            return GetType()
                .GetComponentPropertiesCached()
                .Select(componentProperty => componentProperty?.GetValue(this))
                .OfType<object>();
        }
    }

    /// <summary>
    /// Extensions for composites.
    /// </summary>
    public static class CompositeExtensions
    {
        /// <summary>
        /// Gets all properties from all <see cref="IHas{T}"/> interfaces of the composite type.
        /// Uses cache to avoid reflection stuff.
        /// </summary>
        /// <param name="compositeType">The composite type.</param>
        /// <returns>Components getter properties.</returns>
        public static PropertyInfo[] GetComponentPropertiesCached(this Type compositeType)
        {
            return TwoLayerCache
                .Instance<Type, PropertyInfo[]>("ComponentProperties")
                .GetOrAdd(compositeType, type => GetComponentProperties(type));
        }

        /// <summary>
        /// Gets all properties from all <see cref="IHas{T}"/> interfaces of the composite type.
        /// </summary>
        /// <param name="compositeType">The composite type.</param>
        /// <returns>Components getter properties.</returns>
        public static PropertyInfo[] GetComponentProperties(this Type compositeType) =>
            compositeType
                .GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IHas<>))
                .Select(hasComponentType => hasComponentType.GetProperty(nameof(IHas<object>.Component), returnType: hasComponentType.GetGenericArguments()[0]))
                .Where(pi => pi != null)
                .ToArray();

        /// <summary>
        /// Creates new composite of type <typeparamref name="TComposite"/> and builds it with components of the source composite.
        /// </summary>
        /// <typeparam name="TComposite">Target composite type.</typeparam>
        /// <param name="source">The source composite.</param>
        /// <returns>Result composite.</returns>
        public static TComposite BuildAs<TComposite>(this IComposite source)
            where TComposite : ICompositeBuilder, new()
        {
            return source.BuildAs<TComposite>(factory: () => new TComposite());
        }

        /// <summary>
        /// Creates new composite of type <typeparamref name="TComposite"/> and builds it with components of the source composite.
        /// </summary>
        /// <typeparam name="TComposite">Target composite type.</typeparam>
        /// <param name="source">The source composite.</param>
        /// <param name="factory">Factory that creates initial composite.</param>
        /// <returns>Result composite.</returns>
        public static TComposite BuildAs<TComposite>(this IComposite source, Func<TComposite> factory)
            where TComposite : ICompositeBuilder
        {
            TComposite compositeBuilder = factory();

            foreach (object component in source.Components())
            {
                compositeBuilder = (TComposite)BuildWithUntyped(compositeBuilder, component);
            }

            return compositeBuilder;
        }

        public static object BuildWithUntyped(this object composite, object component)
        {
            Type compositeType = composite.AssertArgumentNotNull(nameof(composite)).GetType();
            Type componentType = component.AssertArgumentNotNull(nameof(component)).GetType();

            var withComponent = TwoLayerCache
                .Instance<(Type, Type), Func<object, object, object>?>("BuildWithCompiledFunc")
                .GetOrAdd((compositeType, componentType), tuple => WithComponentFunc(tuple.Item1, tuple.Item2));

            if (withComponent != null)
            {
                composite = withComponent.Invoke(composite, component);
            }

            return composite;
        }

        private static Func<object, object, object>? WithComponentFunc(Type compositeType, Type componentType)
        {
            Type builderType = typeof(ICompositeBuilder<>).MakeGenericType(componentType);
            bool hasBuilderForComponent = compositeType.IsAssignableTo(builderType);
            if (hasBuilderForComponent)
            {
                MethodInfo methodInfo = typeof(CompositeExtensions)
                    .GetMethod(nameof(WithComponent), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod(compositeType, componentType);

                ParameterExpression inst = Expression.Parameter(typeof(object), "composite");
                ParameterExpression arg0 = Expression.Parameter(typeof(object), "component");
                MethodCallExpression callExpression = Expression.Call(methodInfo, inst, arg0);
                Func<object, object, object> withComponent = Expression
                    .Lambda<Func<object, object, object>>(callExpression, inst, arg0)
                    .Compile();

                return withComponent;
            }

            return null;
        }

        private static object WithComponent<TComposite, TComponent>(object composite, object component)
            where TComposite : ICompositeBuilder<TComponent>
        {
            return ((TComposite)composite).With((TComponent)component);
        }
    }
}
