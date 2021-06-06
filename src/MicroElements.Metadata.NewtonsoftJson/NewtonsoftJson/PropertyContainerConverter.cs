// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MicroElements.Metadata.NewtonsoftJson
{
    /// <summary>
    /// Serializes <see cref="IPropertyContainer"/> as ordinary object.
    /// - Properties serializes as <c>"PropertyName": "PropertyValue"</c>.
    /// - Values serializes according their converters.
    /// <example>
    /// {
    ///   "$metadata.schema.compact": [
    ///     "StringProperty@type=string",
    ///     "IntProperty@type=int",
    ///     "DoubleProperty@type=double",
    ///     "DateProperty@type=LocalDate",
    ///     "StringArray@type=string[]",
    ///     "IntArray@type=int[]"
    ///   ],
    ///   "StringProperty": "Text",
    ///   "IntProperty": 42,
    ///   "DoubleProperty": 10.2,
    ///   "DateProperty": "2020-12-26",
    ///   "StringArray":["a1","a2"],
    ///   "IntArray":[1,2]
    /// }
    /// </example>
    /// </summary>
    public class PropertyContainerConverter : JsonConverter<IPropertyContainer>
    {
        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerConverter"/> class.
        /// </summary>
        /// <param name="options">Metadata json serializer options.</param>
        public PropertyContainerConverter(MetadataJsonSerializationOptions? options = null)
        {
            Options = options.Copy();
        }

        /// <inheritdoc />
        public override IPropertyContainer ReadJson(
            JsonReader reader,
            Type objectType,
            IPropertyContainer? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            IPropertySet? schema = null;
            bool hasSchemaFromType = false;
            bool hasSchemaFromJson = false;

            var propertyContainer = new MutablePropertyContainer();

            IPropertySet? knownPropertySet = objectType.GetSchemaByKnownPropertySet();
            if (knownPropertySet != null)
            {
                schema = knownPropertySet;
                hasSchemaFromType = true;
            }

            ISchemaRepository? schemaRepository = reader.AsMetadataProvider().GetMetadata<ISchemaRepository>();

            if (Options.ReadSchemaFirst)
            {
                JObject jObject = JObject.Load(reader);

                JProperty? jProperty = jObject.Property("$metadata.schema.compact");
                if (jProperty is { First: { } schemaBody })
                {
                    JsonReader jsonReader = schemaBody.CreateReader();
                    var compactSchemaItems = serializer.Deserialize<string[]>(jsonReader);
                    IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                    schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(schemaFromJson) : schemaFromJson;
                    hasSchemaFromJson = true;
                }

                // recreate reader to read from beginning
                reader = jObject.CreateReader();
                reader.Read();
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    TryReadProperty();
                }
                else
                {
                    break;
                }
            }

            void TryReadProperty()
            {
                try
                {
                    ReadProperty();
                }
                catch (Exception e)
                {
                    if (!Options.DoNotFail)
                        throw;
                }
            }

            void ReadProperty()
            {
                string propertyName = (string)reader.Value!;
                Type? propertyType = null;
                IProperty? property = null;

                // Advance reader to property value.
                reader.Read();

                if (propertyName == "$ref")
                {
                    string? jsonReference = serializer.Deserialize<string>(reader);
                    if (jsonReference != null)
                    {
                        IObjectSchema? referencedSchema = (schemaRepository?.GetSchema(jsonReference) as IObjectSchema);
                        if (referencedSchema != null)
                        {
                            PropertySet propertySet = new PropertySet(referencedSchema.Properties);
                            schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(propertySet) : propertySet;
                            hasSchemaFromJson = true;
                        }
                    }

                    return;
                }

                // Compact schema presentation. Use for embedding to json.
                // NOTE: Some json implementations can change property order so $metadata.schema.compact can be not first!!!...
                // For example PostrgeSql jsonb orders properties by name length.
                if (propertyName == "$metadata.schema.compact")
                {
                    if (hasSchemaFromJson)
                    {
                        reader.Skip();
                    }
                    else
                    {
                        var compactSchemaItems = serializer.Deserialize<string[]>(reader);
                        IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                        schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(schemaFromJson) : schemaFromJson;
                        hasSchemaFromJson = true;
                    }

                    return;
                }

                if (propertyType == null && schema != null)
                {
                    property = schema.GetFromSchema(propertyName);
                    propertyType = property?.Type;
                }

                if (propertyType == null)
                {
                    propertyType = GetPropertyTypeFromToken(reader);
                }

                object? propertyValue = serializer.Deserialize(reader, propertyType);
                if (property == null)
                {
                    property = Property.Create(propertyType, propertyName);
                    if (Options.AddSchemaInfo)
                        property.SetIsNotFromSchema();
                }

                propertyContainer.WithValueUntyped(property, propertyValue);
            }

            var resultContainer = propertyContainer.ToPropertyContainerOfType(objectType);
            if (Options.AddSchemaInfo && schema != null)
                resultContainer.SetSchema(schema.ToSchema());
            return resultContainer;
        }

        private Type GetPropertyTypeFromToken(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return typeof(string);
                case JsonToken.Integer:
                    return typeof(int);
                case JsonToken.Float:
                    return typeof(double);
                case JsonToken.Boolean:
                    return typeof(bool);
                default:
                    return typeof(object);
            }
        }

        internal class CompactSchemaGenerator
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

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IPropertyContainer? container, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (container != null && container.Properties.Count > 0)
            {
                bool writeSchemaCompact = Options.WriteSchemaCompact;

                NamingStrategy namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy ?? new DefaultNamingStrategy();
                string GetJsonPropertyName(string name) => namingStrategy.GetPropertyName(name, false);

                ISchemaRepository? schemaRepository = writer.AsMetadataProvider().GetMetadata<ISchemaRepository>();

                if (schemaRepository != null)
                {
                    IObjectSchema objectSchema = container.GetOrCreateSchema();
                    string schemaName = schemaRepository.AddSchema(writer.Path, objectSchema);
                    writer.WritePropertyName("$ref");

                    string schemasRootName = "$defs";
                    string root = $"#/{schemasRootName}/";
                    string referenceName = root + schemaName;

                    writer.WriteValue(referenceName);

                    writeSchemaCompact = false;
                }

                if (writeSchemaCompact)
                {
                    writer.WritePropertyName("$metadata.schema.compact");
                    string[] compactSchema = MetadataSchema.GenerateCompactSchema(container, GetJsonPropertyName, Options.Separator, Options.TypeMapper);
                    serializer.Serialize(writer, compactSchema);
                }

                foreach (IPropertyValue propertyValue in container.Properties)
                {
                    string jsonPropertyName = namingStrategy.GetPropertyName(propertyValue.PropertyUntyped.Name, false);
                    Type propertyType = propertyValue.PropertyUntyped.Type;

                    // PropertyName
                    writer.WritePropertyName(jsonPropertyName);

                    if (Options.WriteArraysInOneRow && propertyType.IsArray)
                        writer.Formatting = Formatting.None;

                    // PropertyValue
                    serializer.Serialize(writer, propertyValue.ValueUntyped, propertyType);

                    // Restore formatting
                    writer.Formatting = serializer.Formatting;
                }
            }

            writer.WriteEndObject();
        }
    }

    public class MetadataSchemaProviderConverter : JsonConverter<IMetadataSchemaProvider>
    {
        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <inheritdoc />
        public MetadataSchemaProviderConverter(MetadataJsonSerializationOptions options)
        {
            Options = options;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IMetadataSchemaProvider? value, JsonSerializer serializer)
        {
            // Remove this converter to deserialize object standard way.
            serializer.Converters.Remove(this);

            SchemaRepository schemaRepository = new SchemaRepository();
            JTokenWriter jTokenWriter = new JTokenWriter();

            // Attaches ISchemaRepository to reader
            jTokenWriter.AsMetadataProvider().SetMetadata((ISchemaRepository)schemaRepository);

            serializer.Serialize(jTokenWriter, value);
            JToken jToken = jTokenWriter.Token;

            bool writeSchemasSection = true;
            if (writeSchemasSection)
            {
                JObject defsContent = new JObject();
                JToken defsProperty = new JProperty("$defs", defsContent);

                foreach (KeyValuePair<string, ISchema> valuePair in schemaRepository.GetSchemas())
                {
                    object schema = new PropertyContainerConverter.CompactSchemaGenerator(serializer, Options).GenerateSchema(valuePair.Value);
                    JProperty schemaProperty = new JProperty(valuePair.Key, new JObject(new JProperty("$metadata.schema.compact", JArray.FromObject(schema))));
                    defsContent.Add(schemaProperty);
                }

                jToken.Last.AddAfterSelf(defsProperty);
            }

            jToken.WriteTo(writer);
        }

        /// <inheritdoc />
        public override IMetadataSchemaProvider ReadJson(
            JsonReader reader,
            Type objectType,
            IMetadataSchemaProvider? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            string schemasRootName = "$defs";

            ISchemaRepository? schemaRepository = reader.AsMetadataProvider().GetMetadata<ISchemaRepository>();
            if (schemaRepository == null)
            {
                schemaRepository = new SchemaRepository();

                // Load json in memory (first iteration).
                JObject jObject = JObject.Load(reader);
                if (jObject.Property(schemasRootName)?.Value is JObject schemasRoot)
                {
                    foreach (JProperty property in schemasRoot.Properties())
                    {
                        string schemaName = property.Name;
                        string root = $"#/{schemasRootName}/";
                        string referenceName = root + schemaName;

                        JProperty? jProperty = ((JObject)property.Value).Property("$metadata.schema.compact");
                        if (jProperty is { First: { } schemaBody })
                        {
                            JsonReader jsonReader = schemaBody.CreateReader();
                            var compactSchemaItems = serializer.Deserialize<string[]>(jsonReader);
                            IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                            schemaRepository.AddSchema(referenceName, schemaFromJson.ToSchema());
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

    public class SchemaRepositoryConverter : JsonConverter<ISchemaRepository>
    {
        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <inheritdoc />
        public SchemaRepositoryConverter(MetadataJsonSerializationOptions options)
        {
            Options = options;
        }

        /// <inheritdoc />
        public override void WriteJson(
            JsonWriter writer,
            ISchemaRepository value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ISchemaRepository ReadJson(
            JsonReader reader,
            Type objectType,
            ISchemaRepository existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            ISchemaRepository schemaRepository = new SchemaRepository();

            JObject jObject = JObject.Load(reader);

            foreach (JProperty property in jObject.Properties())
            {
                string propertyName = property.Name;
                string root = "#/$defs/";
                string referenceName = root + propertyName;

                JProperty? jProperty = ((JObject)property.Value).Property("$metadata.schema.compact");
                if (jProperty is { First: { } schemaBody })
                {
                    JsonReader jsonReader = schemaBody.CreateReader();
                    var compactSchemaItems = serializer.Deserialize<string[]>(jsonReader);
                    IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                    schemaRepository.AddSchema(referenceName, schemaFromJson.ToSchema());
                }
            }

            reader.AsMetadataProvider().SetMetadata((ISchemaRepository)schemaRepository);

            return schemaRepository;
        }
    }
}
