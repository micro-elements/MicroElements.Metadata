// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroElements.Functional;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.SystemTextJson
{
    /// <summary>
    /// Creates strong typed instances of <see cref="PropertyContainerConverter{TPropertyContainer}"/>.
    /// </summary>
    public class PropertyContainerConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerConverterFactory"/> class.
        /// </summary>
        /// <param name="options">Metadata json serializer options.</param>
        public PropertyContainerConverterFactory(MetadataJsonSerializationOptions? options = null)
        {
            Options = options.Copy();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo<IPropertyContainer>();
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // JsonConverter converter = new PropertyContainerConverter<T>(Options);
            Type genericConverterType = typeof(PropertyContainerConverter<>).MakeGenericType(typeToConvert);
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(genericConverterType, Options);

            return converter;
        }
    }
}
