// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.CompositeBuilder
{
    /// <summary>
    /// Schema builder interface.
    /// </summary>
    public interface ICompositeBuilder
    {
    }

    /// <summary>
    /// Schema builder with knowledge of metadata type.
    /// </summary>
    /// <typeparam name="TBuildData">Metadata type.</typeparam>
    public interface ICompositeBuilder<in TBuildData> : ICompositeBuilder
    {
        /// <summary>
        /// Creates a copy of the source with provided <paramref name="buildData"/>.
        /// </summary>
        /// <param name="buildData">Schema part.</param>
        /// <returns>A copy of the source.</returns>
        object With(TBuildData buildData);
    }

    public interface ICompositeSetter<in TPart> : ICompositeBuilder
    {
        /// <summary>
        /// Creates a copy of the source with provided <paramref name="part"/>.
        /// </summary>
        /// <param name="part">Schema part.</param>
        void Append(TPart part);
    }

    /// <summary>
    /// Strong typed schema builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TComposite">Schema type.</typeparam>
    /// <typeparam name="TBuildData">Schema part.</typeparam>
    public interface ICompositeBuilder<out TComposite, in TBuildData> : ICompositeBuilder<TBuildData>
    {
        /// <inheritdoc />
        object ICompositeBuilder<TBuildData>.With(TBuildData buildData) => With(buildData);

        /// <summary>
        /// Creates a copy of the source with provided <paramref name="buildData"/>.
        /// </summary>
        /// <param name="buildData">Schema part.</param>
        /// <returns>Schema copy.</returns>
        new TComposite With(TBuildData buildData);
    }


    public interface IConfigure
    {
        Type OptionType { get; }
    }

    public interface IConfigure<in TOptions> : IConfigure
    {
        /// <inheritdoc />
        Type IConfigure.OptionType => typeof(TOptions);

        void Configure(TOptions options);
    }

    class Configure<TOptions> : IConfigure<TOptions>
    {
        private readonly Action<TOptions> _configure;

        public Configure(Action<TOptions> configure) => _configure = configure;

        /// <inheritdoc />
        void IConfigure<TOptions>.Configure(TOptions options) => _configure(options);
    }

    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static partial class CompositeBuilder
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
            where TSchema : ICompositeBuilder<TComponent>
        {
            schema.AssertArgumentNotNull(nameof(schema));
            component.AssertArgumentNotNull(nameof(component));

            // More specific interface implemented
            if (schema is ICompositeBuilder<TSchema, TComponent> schemaBuilder)
            {
                return schemaBuilder.With(component);
            }

            // Less specific interface
            ICompositeBuilder<TComponent> schemaBuilderUntyped = schema;
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

            // ISchema can inherit metadata from other schema.
            if (source is ISchema schema)
                return schema.GetSchemaMetadata<TComponent>();

            // Get from metadata.
            if (source is IMetadataProvider metadataProvider)
                return metadataProvider.GetMetadata<TComponent>();

            // Last chance: every object can have an attached metadata.
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
