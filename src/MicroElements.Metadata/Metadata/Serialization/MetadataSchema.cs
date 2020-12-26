using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Shared;

namespace MicroElements.Metadata.Serialization
{
    public static class MetadataSchema
    {
        public static string[] GenerateCompactSchema(IPropertyContainer propertyContainer, Func<string, string> gatPropertyName, string separator = "@")
        {
            string[] propertyInfos = new string[propertyContainer.Properties.Count];
            int i = 0;
            foreach (IPropertyValue propertyValue in propertyContainer.Properties)
            {
                string jsonPropertyName = gatPropertyName(propertyValue.PropertyUntyped.Name);
                Type propertyType = propertyValue.PropertyUntyped.Type;
                string typeAlias = DefaultMapperSettings.TypeCache.GetAliasForType(propertyType) ?? propertyType.FullName;
                propertyInfos[i++] = $"{jsonPropertyName}{separator}type={typeAlias}";
            }

            return propertyInfos;
        }

        public static IPropertySet ParseCompactSchema(string[]? compactSchemaItems, string separator = "@")
        {
            if (compactSchemaItems == null)
                return new PropertySet();

            List<IProperty> properties = new List<IProperty>(compactSchemaItems.Length);
            foreach (string compactSchemaItem in compactSchemaItems)
            {
                IProperty? property = ParsePropertyInfo(compactSchemaItem, separator);
                properties.Add(property);
            }

            return new PropertySet(properties);
        }

        public static IPropertySet ParseMetadataTypes(string[]? typeNames)
        {
            if (typeNames == null)
                return new PropertySet();

            typeNames.Select(typeName => DefaultMapperSettings.TypeCache.GetByAliasOrFullName(typeName)).ToArray();

            List<IProperty> properties = new List<IProperty>(typeNames.Length);
            int i = 0;
            foreach (string typeName in typeNames)
            {
                Type type = DefaultMapperSettings.TypeCache.GetByAliasOrFullName(typeName);
                IProperty property = Property.Create(type, $"{i}");
                properties.Add(property);
                i++;
            }

            return new PropertySet(properties);
        }

        public static IProperty ParsePropertyInfo(string fullPropertyName, string separator)
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
                        return Property.Create(propertyType, propertyName);
                }
            }

            return null;
        }

        public static Option<(string PropertyName, Type PropertyType)> ParseName(string fullPropertyName, string separator)
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

        public static IProperty? GetFromSchema(this IPropertySet propertySet, string propertyName)
        {
            var properties = propertySet.GetProperties();
            foreach (IProperty property in properties)
            {
                if (property.IsMatchesByNameOrAlias(propertyName, ignoreCase: true))
                {
                    return property;
                }
            }

            return null;
        }
    }

    public class MetadataJsonSerializationOptions
    {
        public bool DoNotFail { get; set; } = true;

        public bool WriteArraysInOneRow { get; set; } = true;

        public bool WriteSchemaCompact { get; set; } = true;

        public string Separator { get; set; } = "@";


        public bool WriteSchemaToPropertyName { get; set; } = false;

        public string AltSeparator { get; set; } = ":";
    }
}
