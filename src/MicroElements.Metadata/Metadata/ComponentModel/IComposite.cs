// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.Collections.TwoLayerCache;
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
        IEnumerable<object> GetComponents()
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
        /// Gets the components for the composite.
        /// </summary>
        /// <param name="composite">Source composite.</param>
        /// <returns>Enumeration of components.</returns>
        public static IEnumerable<object> GetComponents(this IComposite composite)
        {
            return composite.GetComponents();
        }

        /// <summary>
        /// Gets the components for the composite merged with metadata components.
        /// </summary>
        /// <param name="composite">The source composite.</param>
        /// <returns>Enumeration of components.</returns>
        public static IEnumerable<object> GetComponentsAndMetadata(this IComposite composite)
        {
            IEnumerable<object> fromMetadata = composite is IMetadataProvider prov ? prov.GetMetadataContainer().Properties.Select(value => value.ValueUntyped).OfType<object>() : Enumerable.Empty<object>();
            return composite.GetComponents().Concat(fromMetadata);
        }

        /// <summary>
        /// Creates new composite of type <typeparamref name="TComposite"/> and builds it with components of the source composite.
        /// </summary>
        /// <typeparam name="TComposite">Target composite type.</typeparam>
        /// <param name="source">The source composite.</param>
        /// <param name="factory">Factory that creates initial composite.</param>
        /// <returns>Result composite.</returns>
        public static TComposite BuildAs<TComposite>(this IComposite source, Func<TComposite>? factory = null)
            where TComposite : ICompositeBuilder
        {
            factory ??= ExpressionExtensions.GetDefaultFactoryCached<TComposite>();

            if (factory is null)
                throw new InvalidOperationException($"Type {typeof(TComposite).Name} should contain default constructor or constructor where all parameters have default values or provide factory method.");

            TComposite compositeBuilder = factory();

            foreach (object component in source.GetComponents())
            {
                compositeBuilder = (TComposite)BuildWithUntyped(compositeBuilder, component);
            }

            return compositeBuilder;
        }

        /// <summary>
        /// Calls `With` method on `composite` if the composite type implements needed <see cref="ICompositeBuilder"/>.
        /// </summary>
        /// <param name="composite">Composite.</param>
        /// <param name="component">Component to add.</param>
        /// <returns>Result composite.</returns>
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

    internal static class ExpressionExtensions
    {
        internal static Func<T>? GetDefaultFactory<T>() =>
            GetDefaultFactory(typeof(T)).Typed<T>();

        internal static Func<T>? Typed<T>(this Func<object>? func) =>
            func != null ? () => (T)func() : null;

        internal static Func<object>? GetDefaultFactory(this Type type)
        {
            Func<object>? factory = null;

            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            ConstructorInfo? defaultCtor = constructorInfos.FirstOrDefault(info => info.GetParameters().Length == 0);
            if (defaultCtor != null)
            {
                factory = () => Activator.CreateInstance(type);
            }
            else
            {
                ConstructorInfo? ctorWithAllDefaults = constructorInfos.FirstOrDefault(info => info.GetParameters() is { Length: > 0 } args && args.All(parameterInfo => parameterInfo.HasDefaultValue));
                if (ctorWithAllDefaults != null)
                {
                    object[] ctorArgs = ctorWithAllDefaults.GetParameters().Select(pi => pi.DefaultValue).ToArray();
                    factory = () => Activator.CreateInstance(type, ctorArgs);
                }
            }

            return factory;
        }

        internal static Func<T>? GetDefaultFactoryCached<T>() =>
            GetDefaultFactoryCached(typeof(T)).Typed<T>();

        internal static Func<object>? GetDefaultFactoryCached(this Type type) =>
            TwoLayerCache
                .Instance<Type, Func<object>?>("GetDefaultFactoryCache")
                .GetOrAdd(type, type => GetDefaultFactory(type));
    }
}
