// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder interface.
    /// </summary>
    public interface ISchemaBuilder
    {
    }

    /// <summary>
    /// Strong typed schema builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    /// <typeparam name="TMetadata">Schema part.</typeparam>
    public interface ISchemaBuilder<TSchema, TMetadata> : ISchemaBuilder
        where TSchema : ISchema
        where TMetadata : IMetadata
    {
        /// <summary>
        /// Creates copy of source schema with provided <paramref name="schemaPart"/>.
        /// </summary>
        /// <param name="schemaPart">Schema part.</param>
        /// <returns>Schema copy.</returns>
        TSchema With(TMetadata schemaPart);
    }

    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static class SchemaBuilderExtensions
    {
        /// <summary>
        /// Creates schema copy with provided description.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="description">Description.</param>
        /// <returns>New schema instance with provided description.</returns>
        public static TSchema WithDescription<TSchema>(this TSchema source, string description)
            where TSchema : ISchemaBuilder<TSchema, ISchemaDescription>, ISchema
        {
            return source.With(new SchemaDescription(description));
        }
    }
}
