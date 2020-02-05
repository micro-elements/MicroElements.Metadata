// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has metadata.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for current instance.
        /// </summary>
        IPropertyContainer Metadata => this.GetInstanceMetadata();
    }

    /// <summary>
    /// Global metadata cache.
    /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata for objects.
    /// </summary>
    public static class MetadataGlobalCache
    {
        private static readonly ConditionalWeakTable<object, IPropertyContainer> MetadataCache = new ConditionalWeakTable<object, IPropertyContainer>();

        /// <summary>
        /// Gets metadata for <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>Metadata for instance.</returns>
        public static IPropertyContainer GetInstanceMetadata(this object instance)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (!MetadataCache.TryGetValue(instance, out IPropertyContainer propertyList))
            {
                propertyList = new MutablePropertyContainer();
                MetadataCache.Add(instance, propertyList);
            }

            return propertyList;
        }
    }

    /// <summary>
    /// Provides extension methods for metadata providers.
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets metadata of some type.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <returns>Metadata or default value if not found.</returns>
        public static TMetadata GetMetadata<TMetadata>(this IMetadataProvider metadataProvider, string metadataName = null)
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

            metadataName ??= typeof(TMetadata).FullName;
            var metadata = metadataProvider.Metadata ?? metadataProvider.GetInstanceMetadata();

            var propertyValue = metadata.GetPropertyValue<TMetadata>(Search.ByNameOrAlias(metadataName, ignoreCase: true));
            if (propertyValue != null)
                return propertyValue.Value;

            return default;
        }

        /// <summary>
        /// Sets metadata for item and returns the same metadataProvider for chaining.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="data">Metadata to set.</param>
        /// <returns>The same metadataProvider.</returns>
        public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(this TMetadataProvider metadataProvider, TMetadata data)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.SetMetadata(typeof(TMetadata).FullName, data);
        }

        /// <summary>
        /// Sets metadata for item and returns the same metadataProvider for chaining.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="metadataName">Metadata name.</param>
        /// <param name="data">Metadata to set.</param>
        /// <returns>The same metadataProvider.</returns>
        public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(this TMetadataProvider metadataProvider, string metadataName, TMetadata data)
            where TMetadataProvider : IMetadataProvider
        {
            metadataName ??= typeof(TMetadata).FullName;
            var metadata = metadataProvider.Metadata ?? metadataProvider.GetInstanceMetadata();
            if (metadata is IMutablePropertyContainer mutablePropertyContainer)
            {
                mutablePropertyContainer.SetValue(new Property<TMetadata>(metadataName), data);
            }

            return metadataProvider;
        }
    }
}
