// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static partial class SchemaBuilder
    {
        /// <summary>
        /// Creates a new copy of the source with new default value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="component">Component to add.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithComponent<TSchema, TComponent>(this TSchema schema, TComponent component)
            where TSchema : ISchemaBuilder<TComponent>, ISchema
            where TComponent : IMetadata
        {
            schema.AssertArgumentNotNull(nameof(schema));
            component.AssertArgumentNotNull(nameof(component));

            // More specific interface implemented
            if (schema is ISchemaBuilder<TSchema, TComponent> schemaBuilder)
            {
                return schemaBuilder.With(component);
            }

            // Less specific interface
            ISchemaBuilder<TComponent> schemaBuilderUntyped = schema;
            object copy = schemaBuilderUntyped.With(component);

            // NOTE: No guarantee that return type will be TSchema
            return (TSchema)copy;
        }

        /// <summary>
        /// Gets component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Component or <see langword="null"/> if component was not found.</returns>
        public static TComponent? GetComponent<TComponent>(this object source)
        {
            // Implements IHas<TComponent>. Return component if it is not null because component also can be stored in metadata.
            if (source is IHas<TComponent> { Component: not null } has)
                return has.Component;

            if (source is ISchema schema)
                return schema.GetSchemaMetadata<TComponent>();

            if (source is IMetadataProvider metadataProvider)
                return metadataProvider.GetMetadata<TComponent>();

            if (source.AsMetadataProvider().GetMetadata<TComponent>() is { } fromMetadata)
                return fromMetadata;

            return default;
        }

        /// <summary>
        /// Gets component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Component or <see langword="null"/> if component was not found.</returns>
        public static TComponent? GetSelfOrComponent<TComponent>(this object source)
        {
            if (source is TComponent component)
                return component;

            return source.GetComponent<TComponent>();
        }
    }
}
