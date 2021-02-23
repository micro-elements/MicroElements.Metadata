// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents complex schema for object.
    /// </summary>
    public interface IObjectSchema : ISchema
    {
        /// <summary>
        /// Gets object properties.
        /// </summary>
        IReadOnlyCollection<IProperty> Properties { get; }

        /// <summary>
        /// Gets optional schema combination.
        /// </summary>
        ISchemaCombinator? Combinator { get; }

        /// <summary>
        /// Gets property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Property or null.</returns>
        IProperty? GetProperty(string name);
    }

    /// <summary>
    /// Provides schema.
    /// </summary>
    public interface ISchemaProvider
    {
        /// <summary>
        /// Gets schema instance.
        /// </summary>
        /// <returns>Schema instance.</returns>
        ISchema GetSchema();
    }

    public interface IObjectSchemaProvider : ISchemaProvider
    {
        /// <inheritdoc />
        ISchema ISchemaProvider.GetSchema() => GetObjectSchema();

        /// <summary>
        /// Gets schema instance.
        /// </summary>
        /// <returns>Schema instance.</returns>
        IObjectSchema GetObjectSchema();
    }

    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Gets properties for <see cref="ISchema"/>.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns><see cref="IProperty"/> enumeration.</returns>
        public static IEnumerable<IProperty> GetProperties(this ISchema schema)
        {
            if (schema is IObjectSchema objectSchema)
                return objectSchema.Properties;

            return schema.GetSchemaProperties()?.Properties ?? Array.Empty<IProperty>();
        }

        public static IMutableObjectSchema ToMutableObjectSchema(this ISchema schema)
        {
            if (schema is IMutableObjectSchema objectSchema)
                return objectSchema;

            return new MutableObjectSchema(schema.GetProperties());
        }

        public static IObjectSchema ToObjectSchema(this ISchema schema)
        {
            if (schema is IObjectSchema objectSchema)
                return objectSchema;

            return new MutableObjectSchema(schema.GetProperties());
        }

    }

    /// <summary>
    /// Represents root schema.
    /// </summary>
    public interface IRootSchema : IObjectSchema
    {
        /// <summary>
        /// Gets optional schema definitions.
        /// </summary>
        IReadOnlyDictionary<string, ISchema>? Definitions { get; }
    }

    /*
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "$id": "http://example.com/product.schema.json",

      "title": "Product", //IProperty. Name

      "description": "A product from Acme's catalog", // IProperty.Description
      "type": "object", // IProperty.Type

      "properties": {
        "productId": {
          "description": "The unique identifier for a product",
          "type": "integer"
        },
        "productName": {
          "description": "Name of the product",
          "type": "string"
        },
        "price": {
          "description": "The price of the product",
          "type": "number",
          "exclusiveMinimum": 0
        }
      },

      "required": [ "productId", "productName", "price" ]
    }

 */
}
