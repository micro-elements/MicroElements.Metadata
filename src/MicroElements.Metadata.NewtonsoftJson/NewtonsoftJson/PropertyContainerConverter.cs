// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
                        string schemaId = jsonReference;
                        string root = $"#/{Options.SchemasRootName}/";
                        if (jsonReference.StartsWith(root))
                            schemaId = jsonReference.Substring(root.Length);
                        if (schemaRepository?.GetSchema(schemaId) is IObjectSchema referencedSchema)
                        {
                            schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(referencedSchema) : referencedSchema;
                            hasSchemaFromJson = true;
                        }
                    }

                    return;
                }

                // Compact schema presentation. Use for embedding to json.
                // NOTE: Some json implementations can change property order so $metadata.schema.compact can be not first!!!...
                // For example PostrgeSql jsonb orders properties by name length. Use ReadSchemaFirst for this case.
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

                if (schemaRepository != null && Options.UseSchemasRoot)
                {
                    IObjectSchema objectSchema = container.GetOrCreateSchema();
                    string schemaName = schemaRepository.AddSchema(writer.Path, objectSchema);
                    writer.WritePropertyName("$ref");

                    string root = $"#/{Options.SchemasRootName}/";
                    string referenceName = root + schemaName;

                    writer.WriteValue(referenceName);

                    // No need to write schema because it is in schemas section.
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
}
