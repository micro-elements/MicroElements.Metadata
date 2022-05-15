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
    /// Schema builder with knowledge of metadata type.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    public interface ISchemaBuilder<in TMetadata> : ISchemaBuilder
        where TMetadata : IMetadata
    {
        /// <summary>
        /// Creates a copy of the source with provided <paramref name="schemaPart"/>.
        /// </summary>
        /// <param name="schemaPart">Schema part.</param>
        /// <returns>A copy of the source.</returns>
        object With(TMetadata schemaPart);
    }

    /// <summary>
    /// Strong typed schema builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    /// <typeparam name="TMetadata">Schema part.</typeparam>
    public interface ISchemaBuilder<out TSchema, in TMetadata> : ISchemaBuilder<TMetadata>
        where TSchema : ISchema
        where TMetadata : IMetadata
    {
        /// <inheritdoc />
        object ISchemaBuilder<TMetadata>.With(TMetadata schemaPart) => With(schemaPart);

        /// <summary>
        /// Creates a copy of the source with provided <paramref name="schemaPart"/>.
        /// </summary>
        /// <param name="schemaPart">Schema part.</param>
        /// <returns>Schema copy.</returns>
        new TSchema With(TMetadata schemaPart);
    }
}
