// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides extension methods for metadata providers.
    /// </summary>
    public static class MetadataProviderExtensions
    {
        /// <summary>
        /// Gets metadata of required type.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <param name="defaultValue">Default value to return if not metadata found.</param>
        /// <returns>Metadata or default value if not found.</returns>
        [return: MaybeNull]
        public static TMetadata GetMetadata<TMetadata>(
            this IMetadataProvider metadataProvider,
            string? metadataName = null,
            [AllowNull] TMetadata defaultValue = default)
        {
            return GetMetadataAsOption<TMetadata>(metadataProvider, metadataName)
                .MatchUnsafe(metadata => metadata, defaultValue);
        }

        /// <summary>
        /// Gets metadata as optional value.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <returns>Metadata or default value if not found.</returns>
        public static Option<TMetadata> GetMetadataAsOption<TMetadata>(
            this IMetadataProvider metadataProvider,
            string? metadataName = null)
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

            metadataName ??= typeof(TMetadata).FullName;
            var metadata = metadataProvider.Metadata ?? metadataProvider.GetInstanceMetadata();

            var propertyValue = metadata.GetPropertyValue<TMetadata>(Search
                .ByNameAndComparer<TMetadata>(metadataName, MetadataProvider.DefaultMetadataComparer)
                .ReturnNull());

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
        public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(
            this TMetadataProvider metadataProvider,
            TMetadata data)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.SetMetadata(typeof(TMetadata).FullName, data);
        }

        /// <summary>
        /// Sets metadata for target object and returns the same metadataProvider for chaining.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="metadataName">Metadata name.</param>
        /// <param name="data">Metadata to set.</param>
        /// <returns>The same metadataProvider.</returns>
        public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(
            this TMetadataProvider metadataProvider,
            string? metadataName,
            TMetadata data)
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
        /// Configures metadata with action. Can be called many times.
        /// If metadata is not exists then it creates with default constructor.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="configureMetadata">Configure action.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <returns>The same metadataProvider.</returns>
        public static TMetadataProvider ConfigureMetadata<TMetadataProvider, TMetadata>(
            this TMetadataProvider metadataProvider,
            Action<TMetadata> configureMetadata,
            string? metadataName = null)
            where TMetadataProvider : IMetadataProvider
            where TMetadata : new()
        {
            ConfigureMetadata<TMetadata>(metadataProvider, configureMetadata, metadataName);

            return metadataProvider;
        }

        /// <summary>
        /// Configures metadata with action. Can be called many times.
        /// If metadata is not exists then it creates with default constructor.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProvider">Target metadata provider.</param>
        /// <param name="configureMetadata">Configure action.</param>
        /// <param name="metadataName">Optional metadata name.</param>
        /// <returns>The same metadataProvider.</returns>
        public static IMetadataProvider ConfigureMetadata<TMetadata>(
            this IMetadataProvider metadataProvider,
            Action<TMetadata> configureMetadata,
            string? metadataName = null)
            where TMetadata : new()
        {
            configureMetadata.AssertArgumentNotNull(nameof(configureMetadata));

            metadataProvider.GetMetadataAsOption<TMetadata>(metadataName)
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
