// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroElements.Metadata.NewtonsoftJson
{
    public class CompactSchemaGenerator : IJsonSchemaGenerator, IJsonSchemaSerializer
    {
        private MetadataJsonSerializationOptions _metadataJsonSerializationOptions;
        private Func<string, string> GetJsonPropertyName;

        public CompactSchemaGenerator(
            JsonSerializer jsonSerializer,
            MetadataJsonSerializationOptions metadataJsonSerializationOptions)
        {
            _metadataJsonSerializationOptions = metadataJsonSerializationOptions;

            NamingStrategy namingStrategy = (jsonSerializer.ContractResolver as DefaultContractResolver)?.NamingStrategy ?? new DefaultNamingStrategy();
            GetJsonPropertyName = (name) => namingStrategy.GetPropertyName(name, false);
        }

        /// <inheritdoc />
        IEnumerable<(string Key, object? Value)> IJsonSchemaSerializer.GenerateSchema(ISchema schema)
        {
            var properties = schema.ToObjectSchema().Properties;
            string[] compactSchema = MetadataSchema.GenerateCompactSchema(
                properties,
                GetJsonPropertyName,
                _metadataJsonSerializationOptions.Separator,
                _metadataJsonSerializationOptions.TypeMapper);
            yield return ("$metadata.schema.compact", compactSchema);
        }

        public object GenerateSchema(ISchema schema)
        {
            var properties = schema.ToObjectSchema().Properties;
            string[] compactSchema = MetadataSchema.GenerateCompactSchema(
                properties,
                GetJsonPropertyName,
                _metadataJsonSerializationOptions.Separator,
                _metadataJsonSerializationOptions.TypeMapper);
            return compactSchema;
        }
    }
}
