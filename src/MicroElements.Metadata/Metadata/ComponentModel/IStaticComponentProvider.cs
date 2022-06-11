// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MicroElements.Collections.Cache;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Provides a type that will be used to get components that bound to the type.
    /// </summary>
    /// <typeparam name="T">Type that provides components.</typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Uses in reflection.")]
    public interface IStaticComponentProvider<T>
        where T : IComposite
    {
    }

    /// <summary>
    /// Provides a type that will be used to get metadata that bound to the type.
    /// </summary>
    /// <typeparam name="T">Type that provides metadata.</typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Uses in reflection.")]
    public interface IStaticMetadataProvider<T>
        where T : IMetadataProvider
    {
    }

    /// <summary>
    /// Extensions for static providers.
    /// </summary>
    public static class StaticComponentProvider
    {
        /// <summary>
        /// Gets static components using one or more <see cref="IStaticComponentProvider{T}"/> implemented by type.
        /// </summary>
        /// <param name="type">Type to get components.</param>
        /// <returns>Enumeration of components.</returns>
        public static IEnumerable<object> GetStaticComponents(this Type type)
        {
            return type
                .GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IStaticComponentProvider<>))
                .Select(providerType => providerType.GetGenericArguments().First())
                .Select(argType => argType.GetDefaultFactoryCached()?.Invoke())
                .OfType<IComposite>()
                .SelectMany(composite => composite.GetComponents());
        }

        /// <summary>
        /// Gets static metadata using one or more <see cref="IStaticMetadataProvider{T}"/> implemented by type.
        /// </summary>
        /// <param name="type">Type to get metadata.</param>
        /// <returns>Merged metadata container.</returns>
        public static IPropertyContainer GetStaticMetadataContainer(this Type type)
        {
            var container = type
                .GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IStaticMetadataProvider<>))
                .Select(providerType => providerType.GetGenericArguments().First())
                .Select(argType => argType.GetDefaultFactoryCached()?.Invoke())
                .OfType<IMetadataProvider>()
                .Select(provider => provider.GetMetadataContainer())
                .ToArray()
                .Merge(mergeMode: PropertyAddMode.Set)
                .ToReadOnly();

            return container;
        }

        /// <summary>
        /// Gets static components (cached) using <see cref="IStaticComponentProvider{T}"/> implemented by type.
        /// </summary>
        /// <param name="type">Type to get components.</param>
        /// <returns>Enumeration of components.</returns>
        public static IEnumerable<object> GetStaticComponentsCached(this Type type)
        {
            return Cache
                .Instance<Type, IEnumerable<object>>("StaticComponentsCached")
                .GetOrAdd(type, t => t.GetStaticComponents());
        }

        /// <summary>
        /// Gets static metadata (cached) using one or more <see cref="IStaticMetadataProvider{T}"/> implemented by type.
        /// </summary>
        /// <param name="type">Type to get metadata.</param>
        /// <returns>Merged metadata container.</returns>
        public static IPropertyContainer GetStaticMetadataContainerCached(this Type type)
        {
            return Cache
                .Instance<Type, IPropertyContainer>("StaticMetadataContainerCached")
                .GetOrAdd(type, t => t.GetStaticMetadataContainer());
        }

        /// <summary>
        /// Gets static components (cached) using <see cref="IStaticComponentProvider{T}"/> implemented by the provided object's type.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>Enumeration of components.</returns>
        public static IEnumerable<object> GetStaticComponents(this object source)
        {
            return source.GetType().GetStaticComponentsCached();
        }

        /// <summary>
        /// Gets static metadata (cached) using one or more <see cref="IStaticMetadataProvider{T}"/> implemented by the provided object's type.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>Metadata container.</returns>
        public static IPropertyContainer GetStaticMetadataContainer(this object source)
        {
            return source.GetType().GetStaticMetadataContainerCached();
        }
    }
}
