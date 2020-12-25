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
    public class PropertyContainerWithMetadataTypesConverter : JsonConverter<IPropertyContainer>
    {
        /// <inheritdoc />
        public override IPropertyContainer ReadJson(
            JsonReader reader,
            Type objectType,
            IPropertyContainer? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var propertyContainer = new MutablePropertyContainer();
            Type[]? types = null;

            int propertyIndex = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    reader.Read();
                    if (propertyName == "@metadata.types")
                    {
                        var typeNames = serializer.Deserialize<string[]>(reader);
                        types = typeNames.Select(typeName => DefaultMapperSettings.TypeCache.GetByAliasOrFullName(typeName)).ToArray();
                    }
                    else
                    {
                        Type propertyType = GetPropertyType(reader);
                        object? propertyValue = serializer.Deserialize(reader, propertyType);
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

            Type GetPropertyType(JsonReader reader)
            {
                if (types != null)
                {
                    if (propertyIndex >= types.Length)
                        throw new Exception("@metadata.types out of bound");

                    return types[propertyIndex];
                }
                else
                {
                    return GetPropertyTypeFromToken(reader);
                }
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

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IPropertyContainer? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value != null && value.Properties.Count > 0)
            {
                NamingStrategy? namingStrategy = (serializer.ContractResolver as DefaultContractResolver)?.NamingStrategy;

                string[] types = value.Properties
                    .Select(propertyValue => propertyValue.PropertyUntyped.Type)
                    .Select(type => DefaultMapperSettings.TypeCache.GetAliasForType(type) ?? type.FullName)
                    .ToArray();

                writer.WritePropertyName("@metadata.types");
                serializer.Serialize(writer, types);

                foreach (IPropertyValue propertyValue in value.Properties)
                {
                    string jsonPropertyName = namingStrategy?.GetPropertyName(propertyValue.PropertyUntyped.Name, false) ?? propertyValue.PropertyUntyped.Name;
                    writer.WritePropertyName(jsonPropertyName);
                    serializer.Serialize(writer, propertyValue.ValueUntyped, propertyValue.PropertyUntyped.Type);
                }
            }

            writer.WriteEndObject();
        }
    }
}
