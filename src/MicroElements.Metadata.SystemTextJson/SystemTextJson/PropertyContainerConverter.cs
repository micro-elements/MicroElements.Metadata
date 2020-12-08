// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroElements.Functional;
using MicroElements.Metadata.Serialization;
using MicroElements.Shared;

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
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo<IPropertyContainer>();
        }

        /// <inheritdoc />
        public override IPropertyContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var propertyContainer = new MutablePropertyContainer();
            Type[]? types = null;

            int propertyIndex = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    if (propertyName == "@metadata.types")
                    {
                        var typeNames = JsonSerializer.Deserialize<string[]>(ref reader, options);
                        types = typeNames
                            .Select(typeName => DefaultMapperSettings.TypeCache.GetByAliasOrFullName(typeName))
                            .ToArray();
                    }
                    else
                    {
                        Type propertyType = GetPropertyType(ref reader);
                        object propertyValue = JsonSerializer.Deserialize(ref reader, propertyType, options);
                        IProperty property = Property.Create(propertyType, propertyName);
                        propertyContainer.WithValueUntyped(property, propertyValue);
                        propertyIndex++;
                    }
                }
                else
                {
                    break;
                }
            }

            Type GetPropertyType(ref Utf8JsonReader reader)
            {
                if (types != null)
                {
                    if (propertyIndex >= types.Length)
                        throw new Exception("@metadata.types out of bound");

                    return types[propertyIndex];
                }
                else
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

            if (typeToConvert.IsAssignableTo<IMutablePropertyContainer>())
                return propertyContainer;
            return new PropertyContainer(sourceValues: propertyContainer);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IPropertyContainer value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value != null && value.Properties.Count > 0)
            {
                string[] types = value.Properties
                    .Select(propertyValue => propertyValue.PropertyUntyped.Type)
                    .Select(type => DefaultMapperSettings.TypeCache.GetAliasForType(type) ?? type.FullName)
                    .ToArray();

                writer.WritePropertyName("@metadata.types");
                JsonSerializer.Serialize(writer, types, options);

                foreach (IPropertyValue propertyValue in value.Properties)
                {
                    string jsonPropertyName = options.PropertyNamingPolicy?.ConvertName(propertyValue.PropertyUntyped.Name) ?? propertyValue.PropertyUntyped.Name;
                    writer.WritePropertyName(jsonPropertyName);
                    JsonSerializer.Serialize(writer, propertyValue.ValueUntyped, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
