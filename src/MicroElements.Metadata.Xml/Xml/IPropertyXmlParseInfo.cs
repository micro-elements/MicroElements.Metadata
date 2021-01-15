// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Metadata for property with additional information from xml parser.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IPropertyXmlParseInfo : IMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the property was not created from schema.
        /// </summary>
        bool IsNotFromSchema { get; }
    }

    /// <summary>
    /// Metadata for property with additional information from xml parser.
    /// </summary>
    public class PropertyXmlParseInfo : IPropertyXmlParseInfo
    {
        /// <inheritdoc />
        public bool IsNotFromSchema { get; set; } = true;
    }

    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static class SchemaExtensions
    {
        public static IProperty SetIsNotFromSchema(this IProperty property)
        {
            return property.ConfigureMetadata<IProperty, IPropertyXmlParseInfo, PropertyXmlParseInfo>(info =>
                info.IsNotFromSchema = true);
        }

        public static bool GetIsNotFromSchema(this IProperty property)
        {
            return property.GetMetadata<IPropertyXmlParseInfo>()?.IsNotFromSchema ?? false;
        }

        public static IEnumerable<IProperty> GetPropertiesNotFromSchema(this IPropertySet schema)
        {
            return schema.GetProperties().Where(property => property.GetIsNotFromSchema());
        }
    }
}
