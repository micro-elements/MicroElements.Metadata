// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Composite object builder interface.
    /// </summary>
    public interface ICompositeBuilder
    {
    }

    /// <summary>
    /// Strong typed builder that accepts concrete metadata type.
    /// </summary>
    /// <typeparam name="TComposite">Composite type.</typeparam>
    /// <typeparam name="TPart">Composite part.</typeparam>
    public interface ICompositeBuilder<out TComposite, in TPart> : ICompositeBuilder
    {
        /// <summary>
        /// Creates copy of source object with provided <paramref name="part"/>.
        /// </summary>
        /// <param name="part">Composite part.</param>
        /// <returns>Composite copy (or the same object).</returns>
        TComposite With(TPart part);
    }

    /// <summary>
    /// Marker interface for decorators.
    /// </summary>
    public interface IDecorator
    {
        /// <summary>
        /// Gets type that decorated by this decorator.
        /// </summary>
        Type ComponentType { get; }
    }

    /// <summary>
    /// Marker interface for decorators.
    /// </summary>
    /// <typeparam name="TComponent">Type to decorate.</typeparam>
    public interface IDecorator<out TComponent> : IDecorator
    {
        /// <inheritdoc />
        Type IDecorator.ComponentType => typeof(TComponent);

        /// <summary>
        /// Gets decorated component.
        /// </summary>
        TComponent Component { get; }
    }
}
