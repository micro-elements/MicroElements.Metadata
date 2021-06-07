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
            // Remove this converter to deserialize object standard way.
            serializer.Converters.Remove(this);

            if (!Options.UseSchemasRoot)
            {
                // Serialize standard way
                serializer.Serialize(writer, value);
                return;
            }

            // Get ISchemaRepository from current writer ot create new.
            ISchemaRepository schemaRepository = /*value?.Schemas ?? */writer.AsMetadataProvider().GetMetadata<ISchemaRepository>() ?? new SchemaRepository();

            // Temp writer
            JTokenWriter jTokenWriter = new JTokenWriter();

            // Attaches ISchemaRepository to reader
            jTokenWriter.AsMetadataProvider().SetMetadata((ISchemaRepository)schemaRepository);

            // Stage:1 Write to jTokenWriter
            serializer.Serialize(jTokenWriter, value);
            JToken? jToken = jTokenWriter.Token;

            if (jToken != null)
            {
                JObject defsContent = new JObject();
                JToken defsProperty = new JProperty(Options.SchemasRootName, defsContent);

                foreach (KeyValuePair<string, ISchema> valuePair in schemaRepository.GetSchemas())
                {
                    object schema = new CompactSchemaGenerator(serializer, Options).GenerateSchema(valuePair.Value);
                    JProperty schemaProperty = new JProperty(valuePair.Key, new JObject(new JProperty("$metadata.schema.compact", JArray.FromObject(schema))));
                    defsContent.Add(schemaProperty);
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

                // Remove this converter to deserialize object standard way.
                serializer.Converters.Remove(this);
            }

            // Deserialize object without other hacks (second iteration)
            IMetadataSchemaProvider metadataSchemaProvider = (IMetadataSchemaProvider)serializer.Deserialize(reader, objectType)!;
            return metadataSchemaProvider;
        }
    }
}
