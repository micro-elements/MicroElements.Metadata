// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder interface.
    /// </summary>
    public interface ISchemaBuilder : ICompositeBuilder
    {
    }

    /// <summary>
    /// Schema builder with knowledge of metadata type.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    public interface ISchemaBuilder<in TMetadata> :
        ISchemaBuilder,
        ICompositeBuilder<TMetadata>
        where TMetadata : IMetadata
    {
    }

    /// <summary>
    /// Strong typed schema builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    /// <typeparam name="TMetadata">Schema part.</typeparam>
    public interface ISchemaBuilder<out TSchema, in TMetadata> :
        ISchemaBuilder<TMetadata>, ICompositeBuilder<TSchema, TMetadata>
        where TSchema : ISchema
        where TMetadata : IMetadata
    {
    }
}
