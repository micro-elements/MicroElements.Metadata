// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
        /// Gets properties with values.
        /// </summary>
        IReadOnlyList<IPropertyValue> Metadata { get; }
    }

    public static class Cache
    {
        internal static readonly ConditionalWeakTable<object, PropertyList> Metadata = new ConditionalWeakTable<object, PropertyList>();

        public static PropertyList GetInstanceMetadata(this object instance)
        {
            if (!Metadata.TryGetValue(instance, out PropertyList propertyList))
            {
                propertyList = new PropertyList();
                Metadata.Add(instance, propertyList);
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
            IPropertyValue propertyValue = metadata.GetPropertyByNameOrAlias(metadataName);
            if (propertyValue != null)
                return (TMetadata)propertyValue?.ValueUntyped;

            return default;
        }

        /// <summary>
        /// Sets metadata for item.
        /// </summary>
        /// <typeparam name="TData">Metadata type.</typeparam>
        /// <param name="data">Metadata.</param>
        /// <returns>The same renderer.</returns>
        public static TMetadataProvider SetMetadata<TMetadataProvider, TData>(this TMetadataProvider metadataProvider, TData data) where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.SetMetadata(typeof(TData).FullName, data);
        }

        /// <summary>
        /// Sets metadata for item.
        /// </summary>
        /// <typeparam name="TData">Metadata type.</typeparam>
        /// <param name="name">Metadata name.</param>
        /// <param name="data">Metadata.</param>
        /// <returns>The same renderer.</returns>
        public static TMetadataProvider SetMetadata<TMetadataProvider, TData>(this TMetadataProvider metadataProvider, string name, TData data) where TMetadataProvider : IMetadataProvider
        {
            var metadata = metadataProvider.Metadata ?? metadataProvider.GetInstanceMetadata();
            if (metadata is IMutablePropertyContainer mutablePropertyContainer)
            {
                mutablePropertyContainer.SetValue(new Property<TData>(name), data);
            }

            return metadataProvider;
        }
    }
}
