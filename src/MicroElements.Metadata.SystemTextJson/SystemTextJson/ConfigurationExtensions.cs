// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text.Json;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.SystemTextJson
{
    /// <summary>
    /// Configuration extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configures <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="options"><see cref="JsonSerializerOptions"/> instance.</param>
        /// <param name="configureMetadataJson">Configure Metadata Json serialization.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerOptions ConfigureJsonForPropertyContainers(
            this JsonSerializerOptions options,
            Action<MetadataJsonSerializationOptions>? configureMetadataJson = null)
        {
            bool added = options.Converters.Any(converter => converter.GetType() == typeof(PropertyContainerConverterFactory));
            if (!added)
            {
                MetadataJsonSerializationOptions? metadataJsonSerializationOptions = null;

                if (configureMetadataJson != null)
                {
                    metadataJsonSerializationOptions = new MetadataJsonSerializationOptions();
                    configureMetadataJson(metadataJsonSerializationOptions);
                }

                options.Converters.Add(new PropertyContainerConverterFactory(metadataJsonSerializationOptions));
            }

            return options;
        }

        /// <summary>
        /// Alias for <see cref="ConfigureJsonForPropertyContainers"/>.
        /// Configures <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="options"><see cref="JsonSerializerOptions"/> instance.</param>
        /// <param name="configureMetadataJson">Configure Metadata Json serialization.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerOptions ConfigureForMetadata(
            this JsonSerializerOptions options,
            Action<MetadataJsonSerializationOptions>? configureMetadataJson = null)
        {
            return options.ConfigureJsonForPropertyContainers(configureMetadataJson);
        }
    }
}
