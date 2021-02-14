// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MicroElements.Functional;

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
        /// <returns>Metadata for instance.</returns>
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

            return metadata.GetInstanceMetadata();
        }

        /// <summary>
        /// Gets metadata for <paramref name="instance"/>.
        /// If instance is <see cref="IMetadataProvider"/> then <see cref="IMetadataProvider.Metadata"/> returns.
        /// Otherwise returns <see cref="GetInstanceMetadata"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance.</returns>
        public static IPropertyContainer GetMetadata(this object? instance, bool autoCreate = true)
        {
            return instance switch
            {
                null => PropertyContainer.Empty,
                IMetadataProvider metadataProvider => metadataProvider.GetMetadata(autoCreate),
                _ => instance.GetInstanceMetadata(autoCreate),
            };
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

    /// <summary>
    /// Represents <see cref="IMetadataProvider"/> wrapper for any object.
    /// If object is <see cref="IMetadataProvider"/> then returns <see cref="IMetadataProvider.Metadata"/>,
    /// otherwise returns <see cref="MetadataGlobalCache.GetInstanceMetadata"/>.
    /// </summary>
    public readonly struct MetadataProviderWrapper : IMetadataProvider
    {
        private readonly object _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderWrapper"/> struct.
        /// </summary>
        /// <param name="instance">Object instance.</param>
        public MetadataProviderWrapper(object instance)
        {
            instance.AssertArgumentNotNull(nameof(instance));
            _instance = instance;
        }

        /// <inheritdoc />
        public IPropertyContainer Metadata
        {
            get
            {
                return _instance switch
                {
                    IMetadataProvider metadataProvider => metadataProvider.Metadata,
                    _ => _instance.GetInstanceMetadata(),
                };
            }
        }
    }
}
