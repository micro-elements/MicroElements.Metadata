// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides description for schema, property.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.AnySchema)]
    public interface ISchemaDescription : IMetadata
    {
        /// <summary>
        /// Gets description.
        /// </summary>
        string? Description { get; }
    }

    /// <summary>
    /// Provides description for schema, property.
    /// </summary>
    public record SchemaDescription : ISchemaDescription
    {
        /// <inheritdoc />
        public string? Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDescription"/> class.
        /// </summary>
        /// <param name="description">Description for scheme.</param>
        public SchemaDescription(string? description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static class SchemaBuilderExtensions
    {
        public static IMetadataProvider SetDescription(this IMetadataProvider value, ISchemaDescription description)
        {
            return value.SetMetadata(description);
        }

        public static string? GetDescription(this object value)
        {
            ISchemaDescription? schemaDescription = value.GetComponent<ISchemaDescription>();
            return schemaDescription?.Description;
        }

        /// <summary>
        /// Creates schema copy with provided description.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="description">Description.</param>
        /// <returns>New schema instance with provided description.</returns>
        public static TSchema WithDescription<TSchema>(this TSchema source, string description)
            where TSchema : ICompositeBuilder<TSchema, ISchemaDescription>, ISchema
        {
            return source.With(new SchemaDescription(description));
        }
    }
}
