﻿using System;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroElements.Metadata.NewtonsoftJson
{
    public ref struct CompactSchemaGenerator
    {
        private JsonSerializer _jsonSerializer;
        private MetadataJsonSerializationOptions _metadataJsonSerializationOptions;
        private Func<string, string> GetJsonPropertyName;

        public CompactSchemaGenerator(JsonSerializer jsonSerializer, MetadataJsonSerializationOptions metadataJsonSerializationOptions)
        {
            _jsonSerializer = jsonSerializer;
            _metadataJsonSerializationOptions = metadataJsonSerializationOptions;

            NamingStrategy namingStrategy = (_jsonSerializer.ContractResolver as DefaultContractResolver)?.NamingStrategy ?? new DefaultNamingStrategy();
            GetJsonPropertyName = (name) => namingStrategy.GetPropertyName(name, false);
        }

        public object GenerateSchema(IPropertyContainer container)
        {
            string[] compactSchema = MetadataSchema.GenerateCompactSchema(container, GetJsonPropertyName, _metadataJsonSerializationOptions.Separator, _metadataJsonSerializationOptions.TypeMapper);
            return compactSchema;
        }

        public object GenerateSchema(ISchema schema)
        {
            var properties = schema.ToObjectSchema().Properties;
            string[] compactSchema = MetadataSchema.GenerateCompactSchema(properties, GetJsonPropertyName, _metadataJsonSerializationOptions.Separator, _metadataJsonSerializationOptions.TypeMapper);
            return compactSchema;
        }
    }
}
