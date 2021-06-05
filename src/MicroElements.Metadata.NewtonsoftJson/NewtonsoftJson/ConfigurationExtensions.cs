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
        /// <param name="options"><see cref="JsonSerializerSettings"/> instance.</param>
        /// <param name="configureSerialization">Optional action to configure metadata  serialization.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerSettings ConfigureJsonForPropertyContainers(this JsonSerializerSettings options, Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            MetadataJsonSerializationOptions metadataJsonSerializationOptions = new MetadataJsonSerializationOptions();
            configureSerialization?.Invoke(metadataJsonSerializationOptions);

            options.Converters.Add(new PropertyContainerConverter(metadataJsonSerializationOptions));
            options.Converters.Add(new SchemaRepositoryConverter(metadataJsonSerializationOptions));

            //options.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            return options;
        }

        /// <inheritdoc cref="ConfigureJsonForPropertyContainers"/>.
        public static JsonSerializerSettings ConfigureForMetadata(this JsonSerializerSettings options)
        {
            return options.ConfigureJsonForPropertyContainers();
        }
    }
}
