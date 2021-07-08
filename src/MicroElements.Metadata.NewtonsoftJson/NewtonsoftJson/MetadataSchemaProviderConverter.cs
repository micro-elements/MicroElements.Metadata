// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroElements.Metadata.NewtonsoftJson
{
    /// <summary>
    /// Adds ability to use common json section with schemas ($defs) and replaces $metadata.schema.compact properties with $ref to schema in $defs.
    /// </summary>
    public class MetadataSchemaProviderConverter : JsonConverter<IMetadataSchemaProvider>
    {
        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSchemaProviderConverter"/> class.
        /// </summary>
        /// <param name="options">Serialization options.</param>
        public MetadataSchemaProviderConverter(MetadataJsonSerializationOptions options)
        {
            Options = options.Copy();
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IMetadataSchemaProvider? value, JsonSerializer serializer)
        {
            // Serializer copy without this converter to prevent infinite loop.
            var serializerCopy = serializer.CopyWithoutConverter(this);

            if (!Options.UseSchemasRoot)
            {
                // Serialize standard way
                serializerCopy.Serialize(writer, value);
                return;
            }

            // Get ISchemaRepository from current writer ot create new.
            ISchemaRepository schemaRepository = /*value?.Schemas ?? */writer.AsMetadataProvider().GetMetadata<ISchemaRepository>() ?? new SchemaRepository();

            // Temp writer
            JTokenWriter jTokenWriter = new JTokenWriter();

            // Attaches ISchemaRepository to reader
            jTokenWriter.AsMetadataProvider().SetMetadata((ISchemaRepository)schemaRepository);

            // Stage:1 Write to jTokenWriter
            serializerCopy.Serialize(jTokenWriter, value);

            JToken? jToken = jTokenWriter.Token;

            if (jToken != null)
            {
                JObject defsContent = new JObject();
                JToken defsProperty = new JProperty(Options.SchemasRootName, defsContent);

                foreach (KeyValuePair<string, ISchema> valuePair in schemaRepository.GetSchemas())
                {
                    string schemaId = valuePair.Key;

                    JObject schemaObject = new JObject();
                    JProperty schemaProperty = new JProperty(schemaId, schemaObject);
                    defsContent.Add(schemaProperty);

                    if (Options.WriteSchemaCompact)
                    {
                        object schema = new CompactSchemaGenerator(serializer, Options).GenerateSchema(valuePair.Value);
                        schemaObject.Add(new JProperty("$metadata.schema.compact", JArray.FromObject(schema)));
                    }

                    if (Options.UseJsonSchema)
                    {
                        JObject schemaObject2 = (JObject)new JsonSchemaGenerator(serializer, Options).GenerateSchema(valuePair.Value);
                        schemaObject.Add(schemaObject2.Properties());
                    }
                }

                // Stage2: Add "$defs" section to json.
                jToken.Last?.AddAfterSelf(defsProperty);

                // Stage2: Write final json
                jToken.WriteTo(writer);
            }
        }

        /// <inheritdoc />
        public override IMetadataSchemaProvider ReadJson(
            JsonReader reader,
            Type objectType,
            IMetadataSchemaProvider? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (Options.UseSchemasRoot)
            {
                ISchemaRepository? schemaRepository = reader.AsMetadataProvider().GetMetadata<ISchemaRepository>();
                if (schemaRepository == null)
                {
                    schemaRepository = new SchemaRepository();

                    // Load json in memory (first iteration).
                    JObject jObject = JObject.Load(reader);
                    if (jObject.Property(Options.SchemasRootName)?.Value is JObject schemasRoot)
                    {
                        foreach (JProperty property in schemasRoot.Properties())
                        {
                            string schemaName = property.Name;
                            string root = $"#/{Options.SchemasRootName}/";
                            string referenceName = root + schemaName;

                            JProperty? jProperty = ((JObject)property.Value).Property("$metadata.schema.compact");
                            if (jProperty is { First: { } schemaBody })
                            {
                                JsonReader jsonReader = schemaBody.CreateReader();
                                var compactSchemaItems = serializer.Deserialize<string[]>(jsonReader);
                                IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                                schemaRepository.AddSchema(referenceName, schemaFromJson.ToSchema(schemaName));
                            }
                        }
                    }

                    // Recreate reader to read from beginning (second iteration)
                    reader = jObject.CreateReader();
                    reader.Read();

                    // Attaches ISchemaRepository to reader
                    reader.AsMetadataProvider().SetMetadata((ISchemaRepository)schemaRepository);
                }
            }

            // Remove this converter to deserialize object standard way.
            var serializerCopy = serializer.CopyWithoutConverter(this);

            // Deserialize object without other hacks (second iteration)
            IMetadataSchemaProvider metadataSchemaProvider = (IMetadataSchemaProvider)serializerCopy.Deserialize(reader, objectType)!;

            return metadataSchemaProvider;
        }
    }
}
