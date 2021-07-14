// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using MicroElements.Core;
using MicroElements.Metadata.Mapping;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Global metadata cache.
    /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata for objects.
    /// </summary>
    public static class MetadataGlobalCache
    {
        /// <summary>
        /// ConditionalWeakTable binds metadata to any object adn holds till the object is not GC collected.
        /// </summary>
        private static readonly ConditionalWeakTable<object, IPropertyContainer> _metadataCache = new ();

        /// <summary>
        /// Gets or creates metadata for <paramref name="instance"/>.
        /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata cache.
        /// By default creates <see cref="IMutablePropertyContainer"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance or Empty metadata.</returns>
        public static IPropertyContainer GetInstanceMetadata(this object? instance, bool autoCreate = true)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (!_metadataCache.TryGetValue(instance, out IPropertyContainer metadata))
            {
                if (autoCreate)
                {
                    metadata = new ConcurrentMutablePropertyContainer(searchOptions: MetadataProvider.DefaultSearchOptions);
                    instance.SetInstanceMetadata(metadata);
                }
                else
                {
                    metadata = PropertyContainer.Empty;
                }
            }

            return metadata;
        }

        /// <summary>
        /// Gets or creates metadata for <paramref name="instance"/>.
        /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata cache.
        /// By default creates <see cref="IMutablePropertyContainer"/>.
        /// </summary>
        /// <typeparam name="TContainer">Property container type.</typeparam>
        /// <param name="instance">Source.</param>
        /// <param name="factory">Factory method to create metadata container.</param>
        /// <returns>Metadata for instance or Empty metadata.</returns>
        public static TContainer GetOrCreateInstanceMetadata<TContainer>(this object instance, Func<TContainer> factory)
            where TContainer : IPropertyContainer
        {
            TContainer result;
            if (instance.GetInstanceMetadata(autoCreate: false) is { } metadata)
            {
                result = metadata.ToPropertyContainerOfType<TContainer>(returnTheSameIfNoNeedToConvert: true);
            }
            else
            {
                result = factory();
                instance.SetInstanceMetadata(result);
            }

            return result;
        }

        /// <summary>
        /// Returns a value indicating whether the instance has any metadata.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>A value indicating whether the instance has any metadata.</returns>
        public static bool HasInstanceMetadata(this object? instance)
        {
            if (instance == null)
                return false;

            if (_metadataCache.TryGetValue(instance, out IPropertyContainer metadata) && metadata != null && metadata.Count > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Sets instance metadata in global cache.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <param name="container">Metadata for instance.</param>
        /// <returns>The same container.</returns>
        public static IPropertyContainer SetInstanceMetadata(this object? instance, IPropertyContainer container)
        {
            container.AssertArgumentNotNull(nameof(container));

            if (instance == null)
                return PropertyContainer.Empty;

            _metadataCache.AddOrUpdate(instance, container);

            return container;
        }

        /// <summary>
        /// Replaces <see cref="IMutablePropertyContainer"/> with read only version.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>Current instance metadata.</returns>
        public static IPropertyContainer FreezeInstanceMetadata(this object? instance)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (_metadataCache.TryGetValue(instance, out IPropertyContainer metadata))
            {
                if (metadata is IMutablePropertyContainer)
                {
                    var readOnlyMetadata = metadata.ToReadOnly(flattenHierarchy: true);
                    SetInstanceMetadata(instance, readOnlyMetadata);
                }
            }

            return GetInstanceMetadata(metadata);
        }

        /// <summary>
        /// Returns <see cref="IMetadataProvider"/> for any object.
        /// </summary>
        /// <param name="instance">Object instance.</param>
        /// <returns><see cref="MetadataProviderWrapper"/>.</returns>
        public static MetadataProviderWrapper AsMetadataProvider(this object instance)
        {
            instance.AssertArgumentNotNull(nameof(instance));
            return new MetadataProviderWrapper(instance);
        }
    }
}
