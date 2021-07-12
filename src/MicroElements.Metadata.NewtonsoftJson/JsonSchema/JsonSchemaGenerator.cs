// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.JsonSchema;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MicroElements.Metadata.NewtonsoftJson
{
    public class JsonSchemaGenerator : IJsonSchemaGenerator
    {
        private JsonSerializer _jsonSerializer;
        private MetadataJsonSerializationOptions _metadataJsonSerializationOptions;
        private Func<string, string> GetJsonPropertyName;

        public JsonSchemaGenerator(JsonSerializer jsonSerializer, MetadataJsonSerializationOptions metadataJsonSerializationOptions)
        {
            _jsonSerializer = jsonSerializer;
            _metadataJsonSerializationOptions = metadataJsonSerializationOptions;

            NamingStrategy namingStrategy = (_jsonSerializer.ContractResolver as DefaultContractResolver)?.NamingStrategy ?? new DefaultNamingStrategy();
            GetJsonPropertyName = (name) => namingStrategy.GetPropertyName(name, false);
        }

/*
{
  "type": "object",
  "properties": {
    "first_name": { "type": "string" },
    "last_name": { "type": "string" },
    "shipping_address": { "$ref": "https://example.com/schemas/address" },
    "billing_address": { "$ref": "/schemas/address" }
  },
  "required": ["first_name", "last_name", "shipping_address", "billing_address"]
}
*/

        public object GenerateSchema(ISchema schema)
        {
            // OpenAPI 3.0 uses an extended subset of JSON Schema Specification Wright Draft 00 (aka Draft 5) to describe the data formats.
            // “Extended subset” means that some keywords are supported and some are not, some keywords have slightly different usage
            // than in JSON Schema, and additional keywords are introduced.
            // https://swagger.io/docs/specification/data-models/keywords/

            string jsonMetaSchemaUri = "http://json-schema.org/draft-04/schema";

            JObject schemaObject = new JObject();
            JToken schemaProperty = new JProperty("$schema", jsonMetaSchemaUri);
            JToken typeProperty = new JProperty("type", "object");
            JToken propertiesObject = new JProperty("properties", PropertiesObject());

            schemaObject.Add(schemaProperty);
            schemaObject.Add(typeProperty);
            schemaObject.Add(propertiesObject);

            return schemaObject;

            JObject PropertiesObject()
            {
                var properties = schema.ToObjectSchema().Properties;

                JObject propsObject = new JObject();
                foreach (IProperty property in properties)
                {
                    string propertyName = GetJsonPropertyName(property.Name);
                    Type propertyType = property.Type;

                    // "simpleTypes": {"enum": [ "array", "boolean", "integer", "null", "number", "object", "string" ] }
                    // TODO: evaluate json type
                    string jsonTypeName = JsonTypeMapper.Instance.GetTypeName(propertyType);

                    JObject propertyObject = new JObject(new JProperty("type", jsonTypeName));

                    // TODO: $ref на другие типы
                    if (property.Description != null)
                        propertyObject.Add("description", property.Description);

                    // TODO: xmlObject: https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.3.md#xmlObject

                    // https://swagger.io/docs/specification/data-models/data-types/#null
                    bool isNullAllowed = property.GetOrEvaluateNullability().IsNullAllowed;
                    propertyObject.Add("nullable", isNullAllowed);

                    propsObject.Add(propertyName, propertyObject);
                }

                // TODO: required

                return propsObject;
            }
        }
    }
}
