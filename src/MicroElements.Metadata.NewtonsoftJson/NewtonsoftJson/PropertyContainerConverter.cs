// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Serialization;
using MicroElements.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroElements.Metadata.NewtonsoftJson
{
    /// <summary>
    /// Serializes <see cref="IPropertyContainer"/> as ordinary object.
    /// - Properties serializes as <c>"PropertyName@type=typeName": "PropertyValue"</c>.
    /// - Values serializes according their converters.
    /// <example>
    /// {
    ///     "StringProperty": "Text",
    ///     "IntProperty@type=int": 42
    /// }
    /// </example>
    /// </summary>
    public class PropertyContainerConverter : JsonConverter<IPropertyContainer>
    {
        public bool WriteTypeToName { get; set; } = true;

        public string Separator { get; set; } = "@";

        public string AltSeparator { get; set; } = ":";

        public bool DoNotFail { get; set; } = true;

        /// <inheritdoc />
        public override IPropertyContainer ReadJson(
            JsonReader reader,
            Type objectType,
            IPropertyContainer? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var propertyContainer = new MutablePropertyContainer();

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
                    if (!DoNotFail)
                        throw;
                }
            }

            void ReadProperty()
            {
                string propertyName = (string)reader.Value!;
                reader.Read();

                Type propertyType = null;

                if (propertyName.Contains(Separator))
                {
                    ParseName(propertyName, Separator)
                        .OrElse(ParseName(propertyName, AltSeparator))
                        .MatchSome(result => (propertyName, propertyType) = result);
                }

                if (propertyType == null)
                {
                    propertyType = GetPropertyTypeFromToken(reader);
                }

                object? propertyValue = serializer.Deserialize(reader, propertyType);
                IProperty property = Property.Create(propertyType, propertyName);
                propertyContainer.WithValueUntyped(property, propertyValue);
            }

            Type GetPropertyTypeFromToken(JsonReader reader)
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

            if (objectType.IsAssignableTo<IMutablePropertyContainer>())
                return propertyContainer;

            return new PropertyContainer(sourceValues: propertyContainer.Properties);
        }

        Option<(string PropertyName, Type PropertyType)> ParseName(string fullPropertyName, string separator)
        {
            string[] parts = fullPropertyName.Split(separator);
            if (parts.Length > 1)
            {
                string? typePart = parts.FirstOrDefault(part => part.StartsWith("type="));
                if (typePart != null)
                {
                    string propertyName = parts[0];
                    string typeAlias = typePart.Substring("type=".Length);
                    Type propertyType = DefaultMapperSettings.TypeCache.GetByAliasOrFullName(typeAlias);
                    if (propertyType != null)
                        return (propertyName, propertyType);
                }
            }

            return Option<(string, Type)>.None;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IPropertyContainer? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value != null && value.Properties.Count > 0)
            {
                NamingStrategy? namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy;

                foreach (IPropertyValue propertyValue in value.Properties)
                {
                    string jsonPropertyName = namingStrategy?.GetPropertyName(propertyValue.PropertyUntyped.Name, false) ?? propertyValue.PropertyUntyped.Name;

                    if (WriteTypeToName)
                    {
                        string typeAlias = DefaultMapperSettings.TypeCache.GetAliasForType(propertyValue.PropertyUntyped.Type) ?? propertyValue.PropertyUntyped.Type.FullName;
                        if (typeAlias != "string")
                            jsonPropertyName += $"{Separator}type={typeAlias}";
                    }

                    writer.WritePropertyName(jsonPropertyName);
                    serializer.Serialize(writer, propertyValue.ValueUntyped, propertyValue.PropertyUntyped.Type);
                }
            }

            writer.WriteEndObject();
        }
    }
}
