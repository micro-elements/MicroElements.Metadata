// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides extension methods for metadata providers.
    /// </summary>
    public static class MetadataProviderExtensions
    {
        /// <summary>
        /// Gets metadata of some type.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <param name="defaultValue">Default value to return if not metadata found.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        /// <returns>Metadata or default value if not found.</returns>
        public static TMetadata GetMetadata<TMetadata>(
            this IMetadataProvider metadataProvider,
            string metadataName = null,
            TMetadata defaultValue = default,
            bool ignoreCase = true,
            bool searchInParent = true)
        {
            return GetMetadataAsOption<TMetadata>(metadataProvider, metadataName, ignoreCase, searchInParent)
                .MatchUnsafe(metadata => metadata, defaultValue);
        }

        /// <summary>
        /// Gets metadata as optional value.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        /// <returns>Metadata or default value if not found.</returns>
        public static Option<TMetadata> GetMetadataAsOption<TMetadata>(
            this IMetadataProvider metadataProvider,
            string metadataName = null,
            bool ignoreCase = true,
            bool searchInParent = true)
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

            metadataName ??= typeof(TMetadata).FullName;
            var metadata = metadataProvider.Metadata ?? metadataProvider.GetInstanceMetadata();

            var propertyValue = metadata.GetPropertyValue<TMetadata>(Search.ByNameOrAlias(metadataName, ignoreCase).SearchInParent(searchInParent));
            if (propertyValue.HasValue() && !propertyValue.Value.IsNull())
                return propertyValue.Value;

            return Option<TMetadata>.None;
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

        /// <summary>
        /// Configures metadata with action.
        /// If metadata is not exists then it creates with default constructor.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="configureMetadata">Configure action.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        /// <returns>The same metadataProvider.</returns>
        public static TMetadataProvider ConfigureMetadata<TMetadataProvider, TMetadata>(
            this TMetadataProvider metadataProvider,
            Action<TMetadata> configureMetadata,
            string metadataName = null,
            bool ignoreCase = true,
            bool searchInParent = true)
            where TMetadataProvider : IMetadataProvider
            where TMetadata : new()
        {
            ConfigureMetadata<TMetadata>(metadataProvider, configureMetadata, metadataName, ignoreCase, searchInParent);

            return metadataProvider;
        }

        /// <summary>
        /// Configures metadata with action.
        /// If metadata is not exists then it creates with default constructor.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="configureMetadata">Configure action.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        /// <returns>The same metadataProvider.</returns>
        public static IMetadataProvider ConfigureMetadata<TMetadata>(
            this IMetadataProvider metadataProvider,
            Action<TMetadata> configureMetadata,
            string metadataName = null,
            bool ignoreCase = true,
            bool searchInParent = true)
            where TMetadata : new()
        {
            configureMetadata.AssertArgumentNotNull(nameof(configureMetadata));

            metadataProvider.GetMetadataAsOption<TMetadata>(metadataName, ignoreCase, searchInParent)
                .Match(
                    some: configureMetadata,
                    none: () =>
                    {
                        var metadata = new TMetadata();
                        configureMetadata(metadata);
                        metadataProvider.SetMetadata(metadataName, metadata);
                    });

            return metadataProvider;
        }
    }
}
