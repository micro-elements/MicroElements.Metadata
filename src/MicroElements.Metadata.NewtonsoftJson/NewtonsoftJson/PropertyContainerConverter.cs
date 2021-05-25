// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Serialization;
using Newtonsoft.Json;
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
            bool isPositional = false;
            int propertyIndex = 0;
            var propertyContainer = new MutablePropertyContainer();

            IPropertySet? knownPropertySet = objectType.GetSchemaByKnownPropertySet();
            if (knownPropertySet != null)
                schema = knownPropertySet;

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

                // Compact schema presentation. Use for embedding to json.
                if (propertyName == "$metadata.schema.compact")
                {
                    var compactSchemaItems = serializer.Deserialize<string[]>(reader);
                    IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator);
                    schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(schemaFromJson) : schemaFromJson;
                    return;
                }

                // Obsolete branch. Some json implementations can change property order so @metadata.types can be irrelevant...
                if (propertyName == "@metadata.types")
                {
                    isPositional = true;
                    var typeNames = serializer.Deserialize<string[]>(reader);
                    IPropertySet schemaFromJson = MetadataSchema.ParseMetadataTypes(typeNames);
                    schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(schemaFromJson) : schemaFromJson;
                    return;
                }

                if (propertyType == null && schema != null)
                {
                    if (isPositional)
                        property = schema.GetFromSchema($"{propertyIndex}");
                    else
                        property = schema.GetFromSchema(propertyName);

                    propertyType = property?.Type;
                }

                if (propertyType == null)
                {
                    propertyType = GetPropertyTypeFromToken(reader);
                }

                object? propertyValue = serializer.Deserialize(reader, propertyType);
                property ??= Property.Create(propertyType, propertyName);
                propertyContainer.WithValueUntyped(property, propertyValue);

                propertyIndex++;
            }

            return propertyContainer.ToPropertyContainerOfType(objectType);
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
                NamingStrategy namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy ?? new DefaultNamingStrategy();

                if (Options.WriteSchemaCompact)
                {
                    writer.WritePropertyName("$metadata.schema.compact");
                    string[] compactSchema = MetadataSchema.GenerateCompactSchema(container, s => namingStrategy.GetPropertyName(s, false));
                    serializer.Serialize(writer, compactSchema);
                }

                foreach (IPropertyValue propertyValue in container.Properties)
                {
                    string jsonPropertyName = namingStrategy.GetPropertyName(propertyValue.PropertyUntyped.Name, false);
                    Type propertyType = propertyValue.PropertyUntyped.Type;

                    if (Options.WriteSchemaToPropertyName)
                    {
                        string typeAlias = DefaultMapperSettings.Instance.GetTypeName(propertyType);
                        if (typeAlias != "string")
                            jsonPropertyName += $"{Options.Separator}type={typeAlias}";
                    }

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
