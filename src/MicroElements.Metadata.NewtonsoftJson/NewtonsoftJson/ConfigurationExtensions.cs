// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        /// <returns>The same options.</returns>
        public static JsonSerializerSettings ConfigureJsonForPropertyContainers(this JsonSerializerSettings options)
        {
            options.Converters.Add(new PropertyContainerConverter());
            return options;
        }

        /// <inheritdoc cref="ConfigureJsonForPropertyContainers"/>.
        public static JsonSerializerSettings ConfigureForMetadata(this JsonSerializerSettings options)
        {
            return options.ConfigureJsonForPropertyContainers();
        }
    }
}
