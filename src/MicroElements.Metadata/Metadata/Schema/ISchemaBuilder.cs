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
    public interface ISchemaBuilder<out TSchema, in TMetadata> : ISchemaBuilder
        where TMetadata : IMetadata
    {
        /// <summary>
        /// Creates copy of source schema with provided <paramref name="schemaPart"/>.
        /// </summary>
        /// <param name="schemaPart">Schema part.</param>
        /// <returns>Schema copy.</returns>
        TSchema With(TMetadata schemaPart);
    }
}
