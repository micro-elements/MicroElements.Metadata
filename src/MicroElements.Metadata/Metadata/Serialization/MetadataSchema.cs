// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Generation and parsing for metadata schemas.
    /// </summary>
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
                string typeAlias = DefaultMapperSettings.Instance.GetTypeName(propertyType);
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
                if (property != null)
                    properties.Add(property);
            }

            return new PropertySet(properties);
        }

        public static IPropertySet ParseMetadataTypes(string[]? typeNames)
        {
            if (typeNames == null)
                return new PropertySet();

            List<IProperty> properties = new List<IProperty>(typeNames.Length);
            int i = 0;
            foreach (string typeName in typeNames)
            {
                Type type = DefaultMapperSettings.Instance.GetTypeByName(typeName);
                IProperty property = Property.Create(type, $"{i}");
                properties.Add(property);
                i++;
            }

            return new PropertySet(properties);
        }

        public static IProperty? ParsePropertyInfo(string fullPropertyName, string separator)
        {
            string[] parts = fullPropertyName.Split(separator);
            if (parts.Length > 1)
            {
                string? typePart = parts.FirstOrDefault(part => part.StartsWith("type="));
                if (typePart != null)
                {
                    string propertyName = parts[0];
                    string typeAlias = typePart.Substring("type=".Length);
                    Type? propertyType = DefaultMapperSettings.Instance.GetTypeByName(typeAlias);
                    if (propertyType != null)
                        return Property.Create(propertyType, propertyName);
                }
            }

            return null;
        }

        public static Option<(string PropertyName, Type PropertyType)> ParseName(string fullPropertyName, string? separator)
        {
            if (separator != null)
            {
                string[] parts = fullPropertyName.Split(separator);
                if (parts.Length > 1)
                {
                    string? typePart = parts.FirstOrDefault(part => part.StartsWith("type="));
                    if (typePart != null)
                    {
                        string propertyName = parts[0];
                        string typeAlias = typePart.Substring("type=".Length);
                        Type propertyType = DefaultMapperSettings.Instance.GetTypeByName(typeAlias);
                        if (propertyType != null)
                            return (propertyName, propertyType);
                    }
                }
            }

            return Option<(string, Type)>.None;
        }

        public static IProperty? GetFromSchema(this IPropertySet propertySet, string propertyName)
        {
            var properties = propertySet.GetProperties();
            foreach (IProperty property in properties)
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property;
                }
            }

            return null;
        }

        public static IPropertySet AppendAbsentProperties(this IPropertySet source, IPropertySet propertiesToAdd, IEqualityComparer<IProperty>? propertyComparer = null)
        {
            propertyComparer ??= PropertyComparer.ByTypeAndNameIgnoreCaseComparer;
            var sourceProperties = source.GetProperties().ToList();
            var lookup = sourceProperties.ToDictionary(property => property, property => property, propertyComparer);
            List<IProperty> toAdd = propertiesToAdd.GetProperties().Where(property => !lookup.ContainsKey(property)).ToList();
            if (toAdd.Count > 0)
                return new PropertySet(sourceProperties.Concat(toAdd));
            return source;
        }
    }
}
