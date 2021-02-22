// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema for <see cref="IPropertyContainer"/>.
    /// Contains properties, constraints and validation rules.
    /// </summary>
    public interface ISchema : IPropertySet
    {
        //string Id { get; }// add Description etc

        /// <summary>
        /// Adds property to the schema.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Property that was added.</returns>
        IProperty AddProperty(IProperty property);

        /// <summary>
        /// Gets property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
        IProperty? GetProperty(string name);
    }

    /*
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "$id": "http://example.com/product.schema.json",
          "title": "Product",

          "description": "A product from Acme's catalog",
          "type": "object",

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

    public interface ISchemaNew<T> : IProperty
    {
        IProperty<T> Definition { get; }

        /// <inheritdoc />
        Type IProperty.Type => typeof(T);

        IPropertySet Properties { get; }
    }

    public interface ISchemaNew2 : IMetadataProvider, IMetadata
    {
        string Id { get; }

        /// <summary>
        /// Name, Type
        /// </summary>
        IProperty Definition { get; }

        IPropertyContainer ISchemaDescription { get; }

        IPropertySet Properties { get; }

        object[] Items { get; }

        ICombinator Combinator { get; }
    }
}
