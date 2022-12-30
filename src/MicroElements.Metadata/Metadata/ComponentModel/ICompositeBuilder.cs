// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Exceptions;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Generic builder interface.
    /// </summary>
    public interface ICompositeBuilder
    {
    }

    /// <summary>
    /// Generic builder that knows how to build with the provided component type.
    /// It's supposed to be used with immutable objects.
    /// For mutable objects use <see cref="ICompositeSetter{TComponent}"/>.
    /// </summary>
    /// <typeparam name="TComponent">Component type.</typeparam>
    public interface ICompositeBuilder<in TComponent> : ICompositeBuilder
    {
        /// <summary>
        /// Creates a copy of the source with provided <paramref name="component"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The source copy or the source itself in case of mutable object.</returns>
        object With(TComponent component);
    }

    /// <summary>
    /// Generic builder that knows how to enrich object with some component data.
    /// It's like the <see cref="ICompositeBuilder{TComponent}"/> but for mutable objects.
    /// </summary>
    /// <typeparam name="TComponent">Component type.</typeparam>
    public interface ICompositeSetter<in TComponent> : ICompositeBuilder<TComponent>
    {
        /// <inheritdoc />
        object ICompositeBuilder<TComponent>.With(TComponent component)
        {
            Append(component);
            return this;
        }

        /// <summary>
        /// Creates a copy of the source with provided <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Data to append.</param>
        void Append(TComponent component);
    }

    /// <summary>
    /// Strong typed composite builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TComposite">Composite type.</typeparam>
    /// <typeparam name="TComponent">Component type.</typeparam>
    public interface ICompositeBuilder<out TComposite, in TComponent> : ICompositeBuilder<TComponent>
    {
        /// <inheritdoc />
        object ICompositeBuilder<TComponent>.With(TComponent component) => With(component) ?? throw new InvalidOperationException("Method With should always return not null result.");

        /// <summary>
        /// Creates a copy of the source with provided <paramref name="component"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The source copy or self.</returns>
        new TComposite With(TComponent component);
    }

    /// <summary>
    /// Generic builder extensions.
    /// </summary>
    public static class CompositeBuilder
    {
        /// <summary>
        /// Creates a new copy of the source with the provided component.
        /// Some implementations for example <see cref="ICompositeSetter{TComponent}"/> will return the same mutated instance.
        /// </summary>
        /// <typeparam name="TComposite">Composite type.</typeparam>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source composite.</param>
        /// <param name="component">Component to add.</param>
        /// <returns>The source copy or self.</returns>
        public static TComposite WithComponent<TComposite, TComponent>(this TComposite source, TComponent component)
            where TComposite : ICompositeBuilder<TComponent>
        {
            source.AssertArgumentNotNull(nameof(source));
            component.AssertArgumentNotNull(nameof(component));

            // More specific interface implemented
            if (source is ICompositeBuilder<TComposite, TComponent> compositeBuilder)
            {
                return compositeBuilder.With(component);
            }

            // Less specific interface
            ICompositeBuilder<TComponent> compositeBuilderUntyped = source;
            object composite = compositeBuilderUntyped.With(component);

            // NOTE: No guarantee that return type will be TComposite
            return (TComposite)composite;
        }

        /// <summary>
        /// Sets component for mutable composite.
        /// </summary>
        /// <typeparam name="TComposite">Composite type.</typeparam>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source composite.</param>
        /// <param name="component">Component to add.</param>
        /// <returns>The same instance.</returns>
        public static TComposite SetComponent<TComposite, TComponent>(this TComposite source, TComponent component)
            where TComposite : ICompositeSetter<TComponent>
        {
            source.AssertArgumentNotNull(nameof(source));
            component.AssertArgumentNotNull(nameof(component));

            source.Append(component);

            return source;
        }

        /// <summary>
        /// Gets a component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Component or <see langword="null"/> if component was not found.</returns>
        public static TComponent? GetComponent<TComponent>(this object source)
        {
            //TODO: Checks: source not null, immutability check
            // Implements IHas<TComponent>. Return component if it is not null because component also can be stored in it's metadata.
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
        /// Gets a component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>NotNull component.</returns>
        /// <exception cref="NotFoundException">Component was not found.</exception>
        public static TComponent GetRequiredComponent<TComponent>(this object source)
        {
            source.AssertArgumentNotNull(nameof(source));
            return source.GetComponent<TComponent>() ?? throw new NotFoundException($"The source does not contain component for type {typeof(TComponent).Name}");
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
