// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;

namespace MicroElements.Metadata.NewtonsoftJson
{
    /// <summary>
    /// Configuration extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configures <see cref="JsonSerializerSettings"/> to serialize <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> instance.</param>
        /// <param name="configureSerialization">Optional action to configure metadata serialization.</param>
        /// <param name="metadataJsonSerializationOptions">Optional metadata serialization options.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerSettings ConfigureJsonForPropertyContainers(
            this JsonSerializerSettings jsonSerializerSettings,
            Action<MetadataJsonSerializationOptions>? configureSerialization = null,
            MetadataJsonSerializationOptions? metadataJsonSerializationOptions = null)
        {
            metadataJsonSerializationOptions ??= new MetadataJsonSerializationOptions();
            configureSerialization?.Invoke(metadataJsonSerializationOptions);

            jsonSerializerSettings.Converters.Add(new PropertyContainerConverter(metadataJsonSerializationOptions));
            jsonSerializerSettings.Converters.Add(new MetadataSchemaProviderConverter(metadataJsonSerializationOptions));

            return jsonSerializerSettings;
        }

        /// <summary>
        /// Configures <see cref="JsonSerializerSettings"/> to serialize <see cref="IPropertyContainer"/>.
        /// The same as <see cref="ConfigureJsonForPropertyContainers"/>.
        /// </summary>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> instance.</param>
        /// <param name="configureSerialization">Optional action to configure metadata serialization.</param>
        /// <param name="metadataJsonSerializationOptions">Optional metadata serialization options.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerSettings ConfigureForMetadata(
            this JsonSerializerSettings jsonSerializerSettings,
            Action<MetadataJsonSerializationOptions>? configureSerialization = null,
            MetadataJsonSerializationOptions? metadataJsonSerializationOptions = null)
        {
            return jsonSerializerSettings.ConfigureJsonForPropertyContainers(configureSerialization, metadataJsonSerializationOptions);
        }
    }
}
