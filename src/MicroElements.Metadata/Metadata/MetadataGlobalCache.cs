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
        private static readonly ConditionalWeakTable<object, IPropertyContainer> MetadataCache = new ConditionalWeakTable<object, IPropertyContainer>();

        /// <summary>
        /// Gets or creates metadata for <paramref name="instance"/>.
        /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata cache.
        /// By default creates <see cref="IMutablePropertyContainer"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>Metadata for instance.</returns>
        public static IPropertyContainer GetInstanceMetadata(this object? instance)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (!MetadataCache.TryGetValue(instance, out IPropertyContainer metadata))
            {
                metadata = new MutablePropertyContainer(searchOptions: MetadataProvider.DefaultSearchOptions);
                instance.SetInstanceMetadata(metadata);
            }

            return metadata;
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

            if (instance != null)
                MetadataCache.AddOrUpdate(instance, container);

            return container;
        }

        /// <summary>
        /// Gets metadata for <paramref name="instance"/>.
        /// If instance is <see cref="IMetadataProvider"/> then <see cref="IMetadataProvider.Metadata"/> returns.
        /// Otherwise returns <see cref="GetInstanceMetadata"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>Metadata for instance.</returns>
        public static IPropertyContainer GetMetadata(this object? instance)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (instance is IMetadataProvider metadataProvider)
                return metadataProvider.Metadata;

            return instance.GetInstanceMetadata();
        }
    }
}
