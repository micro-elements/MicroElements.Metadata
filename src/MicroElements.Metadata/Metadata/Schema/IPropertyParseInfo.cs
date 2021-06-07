// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    //TODO: rename to IsFromSchema, make optional

    /// <summary>
    /// Metadata for property with additional information from parser.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IPropertyParseInfo : IMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the property was not created from schema.
        /// </summary>
        bool IsNotFromSchema { get; }
    }

    /// <summary>
    /// Metadata for property with additional information from xml parser.
    /// </summary>
    public class PropertyParseInfo : IPropertyParseInfo
    {
        /// <inheritdoc />
        public bool IsNotFromSchema { get; set; } = true;
    }

    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IPropertyParseInfo.IsNotFromSchema"/> metadata property.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="isNotFromSchema">A value indicating whether the property was not created from schema.</param>
        /// <returns>The same property.</returns>
        public static IProperty SetIsNotFromSchema(this IProperty property, bool isNotFromSchema = true)
        {
            return property.ConfigureMetadata<IProperty, IPropertyParseInfo, PropertyParseInfo>(info =>
                info.IsNotFromSchema = isNotFromSchema);
        }

        /// <summary>
        /// Gets <see cref="IPropertyParseInfo.IsNotFromSchema"/> metadata property.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>A value indicating whether the property was not created from schema.</returns>
        public static bool GetIsNotFromSchema(this IProperty property)
        {
            return property.GetMetadata<IPropertyParseInfo>()?.IsNotFromSchema ?? false;
        }

        /// <summary>
        /// Gets properties that was not created from schema.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <param name="recursive">Recursive search in children schemas.</param>
        /// <returns>Properties that was not created from schema.</returns>
        public static IEnumerable<IProperty> GetPropertiesNotFromSchema(this ISchema schema, bool recursive = true)
        {
            IEnumerable<IProperty> properties = schema.GetProperties();
            foreach (IProperty property in properties)
            {
                if (property.GetIsNotFromSchema())
                    yield return property;
                if (recursive)
                {
                    if (property.GetSchema() is { } subSchema)
                    {
                        foreach (IProperty subProperty in subSchema.GetPropertiesNotFromSchema(recursive))
                        {
                            yield return subProperty;
                        }
                    }
                }
            }
        }
    }
}
