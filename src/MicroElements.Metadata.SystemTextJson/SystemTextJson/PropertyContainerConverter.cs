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
    /// Serializes <see cref="IPropertyContainer"/> as ordinary object.
    /// - Properties serializes as <c>"PropertyName": "PropertyValue"</c>.
    /// - Values serializes according their converters.
    /// <example>
    /// {
    ///     "@metadata.types": ["string", "int"],
    ///     "StringProperty": "Text",
    ///     "IntProperty" : 42
    /// }
    /// </example>
    /// </summary>
    public class PropertyContainerConverter : JsonConverter<IPropertyContainer>
    {
        public MetadataJsonSerializationOptions Options { get; }

        internal PropertyContainerConverter(MetadataJsonSerializationOptions? options = null)
        {
            Options = options ?? new MetadataJsonSerializationOptions();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo<IPropertyContainer>();
        }

        /// <inheritdoc />
        public override IPropertyContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            IPropertySet? schema = null;
            bool isPositional = false;
            int propertyIndex = 0;
            var propertyContainer = new MutablePropertyContainer();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    TryReadProperty(ref reader);
                }
                else
                {
                    break;
                }
            }

            void TryReadProperty(ref Utf8JsonReader reader)
            {
                try
                {
                    ReadProperty(ref reader);
                }
                catch (Exception e)
                {
                    if (!Options.DoNotFail)
                        throw;
                }
            }

            void ReadProperty(ref Utf8JsonReader reader)
            {
                string propertyName = reader.GetString();
                Type? propertyType = null;
                IProperty? property = null;

                // Advance reader to property value.
                reader.Read();

                // Compact schema presentation. Use for embedding to json.
                if (propertyName == "$metadata.schema.compact")
                {
                    var compactSchemaItems = JsonSerializer.Deserialize<string[]>(ref reader, options);
                    schema = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator);
                    return;
                }

                // Obsolete branch. Some json implementations can change property order so @metadata.types can be irrelevant...
                if (propertyName == "@metadata.types")
                {
                    isPositional = true;
                    var typeNames = JsonSerializer.Deserialize<string[]>(ref reader, options);
                    schema = MetadataSchema.ParseMetadataTypes(typeNames);
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
                    propertyType = GetPropertyTypeFromToken(ref reader);
                }

                object? propertyValue = JsonSerializer.Deserialize(ref reader, propertyType, options);
                property ??= Property.Create(propertyType, propertyName);
                propertyContainer.WithValueUntyped(property, propertyValue);

                propertyIndex++;
            }

            if (typeToConvert.IsAssignableTo<IMutablePropertyContainer>())
                return propertyContainer;

            return new PropertyContainer(sourceValues: propertyContainer.Properties);
        }

        private Type GetPropertyTypeFromToken(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return typeof(string);
                case JsonTokenType.Number:
                    return typeof(decimal);
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return typeof(bool);
                default:
                    return typeof(object);
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IPropertyContainer container, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (container != null && container.Properties.Count > 0)
            {
                string GetJsonPropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

                if (Options.WriteSchemaCompact)
                {
                    writer.WritePropertyName("$metadata.schema.compact");
                    string[] compactSchema = MetadataSchema.GenerateCompactSchema(container, GetJsonPropertyName);
                    JsonSerializer.Serialize(writer, compactSchema, options);
                }

                foreach (IPropertyValue propertyValue in container.Properties)
                {
                    string jsonPropertyName = GetJsonPropertyName(propertyValue.PropertyUntyped.Name);
                    Type propertyType = propertyValue.PropertyUntyped.Type;

                    // PropertyName
                    writer.WritePropertyName(jsonPropertyName);

                    if (Options.WriteArraysInOneRow && propertyType.IsArray && writer.Options.Indented)
                    {
                        // Creates NotIndented writer
                        Utf8JsonWriter writerCopy = writer.CloneNotIndented();

                        // PropertyValue
                        JsonSerializer.Serialize(writerCopy, propertyValue.ValueUntyped, options);

                        // Needs to copy internal state back to writer
                        writerCopy.CopyStateTo(writer);
                    }
                    else
                    {
                        // PropertyValue
                        JsonSerializer.Serialize(writer, propertyValue.ValueUntyped, options);
                    }
                }
            }

            writer.WriteEndObject();
        }
    }
}
