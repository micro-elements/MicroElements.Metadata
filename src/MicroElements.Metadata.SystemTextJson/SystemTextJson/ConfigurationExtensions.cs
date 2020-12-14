// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Text.Json;

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
        /// <returns>The same options.</returns>
        public static JsonSerializerOptions ConfigureJsonForPropertyContainers(this JsonSerializerOptions options)
        {
            bool added = options.Converters.FirstOrDefault(converter => converter.GetType() == typeof(PropertyContainerConverter)) != null;
            if (!added)
                options.Converters.Add(new PropertyContainerConverter());
            return options;
        }

        /// <summary>
        /// Alias for <see cref="ConfigureJsonForPropertyContainers"/>.
        /// Configures <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="options"><see cref="JsonSerializerOptions"/> instance.</param>
        /// <returns>The same options.</returns>
        public static JsonSerializerOptions ConfigureForMetadata(this JsonSerializerOptions options)
        {
            return options.ConfigureJsonForPropertyContainers();
        }
    }
}
