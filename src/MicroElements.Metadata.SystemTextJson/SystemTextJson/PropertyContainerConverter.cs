// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroElements.Functional;
using MicroElements.Metadata.Mapping;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.SystemTextJson
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
    /// <typeparam name="TPropertyContainer">PropertyContainer type.</typeparam>
    public class PropertyContainerConverter<TPropertyContainer> : JsonConverter<TPropertyContainer>
        where TPropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Gets output type (for deserialization).
        /// </summary>
        public Type OutputType { get; }

        /// <summary>
        /// Gets metadata json serializer options.
        /// </summary>
        public MetadataJsonSerializationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerConverter{TPropertyContainer}"/> class.
        /// </summary>
        /// <param name="options">Metadata json serializer options.</param>
        public PropertyContainerConverter(MetadataJsonSerializationOptions? options = null)
        {
            OutputType = typeof(TPropertyContainer);
            Options = options.Copy();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(OutputType);
        }

        /// <inheritdoc />
        public override TPropertyContainer Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            IPropertySet? schema = null;
            var propertyContainer = new MutablePropertyContainer();

            IPropertySet? knownPropertySet = typeToConvert.GetSchemaByKnownPropertySet();
            if (knownPropertySet != null)
                schema = knownPropertySet;

            while (utf8JsonReader.Read())
            {
                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName)
                {
                    TryReadProperty(ref utf8JsonReader);
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
                bool autoDetected = false;
                IProperty? property = null;

                // Advance reader to property value.
                reader.Read();

                // Compact schema presentation. Use for embedding to json.
                // NOTE: Some json implementations can change property order so $metadata.schema.compact can be not first!!!...
                if (propertyName == "$metadata.schema.compact")
                {
                    var compactSchemaItems = JsonSerializer.Deserialize<string[]>(ref reader, options);
                    IPropertySet schemaFromJson = MetadataSchema.ParseCompactSchema(compactSchemaItems, Options.Separator, Options.TypeMapper);
                    schema = knownPropertySet != null ? knownPropertySet.AppendAbsentProperties(schemaFromJson) : schemaFromJson;
                    return;
                }

                if (propertyType == null && schema != null)
                {
                    property = schema.GetFromSchema(propertyName);
                    propertyType = property?.Type;
                }

                if (propertyType == null)
                {
                    propertyType = GetPropertyTypeFromToken(ref reader);
                    autoDetected = true;
                }

                object? propertyValue = JsonSerializer.Deserialize(ref reader, propertyType, options);
                if (autoDetected && propertyValue is decimal numeric)
                {
                    bool isInt = numeric % 1 == 0;
                    if (isInt)
                    {
                        propertyType = typeof(int);
                        propertyValue = (int)numeric;
                    }
                    else
                    {
                        propertyType = typeof(double);
                        propertyValue = (double)numeric;
                    }
                }

                if (property == null)
                {
                    property = Property.Create(propertyType, propertyName);
                    if (Options.AddSchemaInfo)
                        property.SetIsNotFromSchema();
                }

                propertyContainer.WithValueUntyped(property, propertyValue);
            }

            var resultContainer = propertyContainer.ToPropertyContainerOfType<TPropertyContainer>();
            if (Options.AddSchemaInfo && schema != null)
                resultContainer.SetSchema(schema.ToSchema());
            return resultContainer;
        }

        public class JsonWriterContext
        {
            private ConcurrentDictionary<Type, bool> Dictionary { get; } = new ConcurrentDictionary<Type, bool>();

            public void SetIsWritten(Type type)
            {
                Dictionary[type] = true;
            }

            public bool IsWritten(Type type)
            {
               return Dictionary.ContainsKey(type);
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TPropertyContainer container, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (container != null && container.Properties.Count > 0)
            {
                string GetJsonPropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

                if (Options.WriteSchemaCompact)
                {
                    Type containerType = container.GetType();

                    bool writeSchemaCompact = true;

                    if (Options.WriteSchemaOnceForKnownTypes && container is IKnownPropertySet)
                    {
                        JsonWriterContext? jsonWriterContext = writer.AsMetadataProvider().GetMetadata<JsonWriterContext>();
                        if (jsonWriterContext != null)
                            writeSchemaCompact = !jsonWriterContext.IsWritten(containerType);
                    }

                    if (writeSchemaCompact)
                    {
                        writer.WritePropertyName("$metadata.schema.compact");
                        string[] compactSchema = MetadataSchema.GenerateCompactSchema(container, GetJsonPropertyName, Options.Separator, Options.TypeMapper);
                        JsonSerializer.Serialize(writer, compactSchema, options);

                        if (Options.WriteSchemaOnceForKnownTypes && container is IKnownPropertySet)
                        {
                            writer.AsMetadataProvider().ConfigureMetadata<JsonWriterContext>(context => context.SetIsWritten(containerType));
                        }
                    }
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
                        JsonSerializer.Serialize(writerCopy, propertyValue.ValueUntyped, propertyType, options);

                        // Needs to copy internal state back to writer
                        writerCopy.CopyStateTo(writer);
                    }
                    else
                    {
                        // PropertyValue
                        JsonSerializer.Serialize(writer, propertyValue.ValueUntyped, propertyType, options);
                    }
                }
            }

            writer.WriteEndObject();
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
    }
}
