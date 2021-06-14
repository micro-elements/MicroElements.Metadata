// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Generation and parsing for metadata schemas.
    /// </summary>
    public static class MetadataSchema
    {
        public static string[] GenerateCompactSchema(
            IPropertyContainer propertyContainer,
            Func<string, string> getPropertyName,
            string separator,
            ITypeMapper? typeMapper)
        {
            List<IProperty> properties = propertyContainer.Properties.Select(pv => pv.PropertyUntyped).ToList();
            return GenerateCompactSchema(properties, getPropertyName, separator, typeMapper);
        }

        public static string[] GenerateCompactSchema(
            IReadOnlyCollection<IProperty> properties,
            Func<string, string> getPropertyName,
            string separator,
            ITypeMapper? typeMapper)
        {
            typeMapper ??= DefaultTypeMapper.Instance;
            string[] propertyInfos = new string[properties.Count];
            int i = 0;
            foreach (IProperty property in properties)
            {
                string jsonPropertyName = getPropertyName(property.Name);
                Type propertyType = property.Type;
                string typeAlias = typeMapper.GetTypeName(propertyType);
                propertyInfos[i++] = $"{jsonPropertyName}{separator}type={typeAlias}";
            }

            return propertyInfos;
        }

        public static IPropertySet ParseCompactSchema(
            string[]? compactSchemaItems,
            string separator,
            ITypeMapper? typeMapper)
        {
            if (compactSchemaItems == null)
                return new PropertySet();

            List<IProperty> properties = new List<IProperty>(compactSchemaItems.Length);
            foreach (string compactSchemaItem in compactSchemaItems)
            {
                IProperty? property = ParsePropertyInfo(compactSchemaItem, separator, typeMapper);
                if (property != null)
                    properties.Add(property);
            }

            return new PropertySet(properties);
        }

        public static IProperty? ParsePropertyInfo(string fullPropertyName, string separator, ITypeMapper? typeMapper)
        {
            typeMapper ??= DefaultTypeMapper.Instance;

            string[] parts = fullPropertyName.Split(separator);
            if (parts.Length > 1)
            {
                string? typePart = parts.FirstOrDefault(part => part.StartsWith("type="));
                if (typePart != null)
                {
                    string propertyName = parts[0];
                    string typeAlias = typePart.Substring("type=".Length);
                    Type? propertyType = typeMapper.GetTypeByName(typeAlias);
                    if (propertyType != null)
                        return Property.Create(propertyType, propertyName);
                }
            }

            return null;
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

        /// <summary>
        /// Merges two property sets.
        /// </summary>
        public static IPropertySet AppendAbsentProperties(this IPropertySet source, IPropertySet propertiesToAdd, IEqualityComparer<IProperty>? propertyComparer = null)
        {
            propertyComparer ??= PropertyComparer.ByNameOrAliasIgnoreCase;

            var sourceProperties = source.GetProperties();
            var resultProperties = sourceProperties.Union(propertiesToAdd.GetProperties(), propertyComparer);

            PropertySet propertySet = new PropertySet(resultProperties);
            return propertySet;
        }
    }
}
