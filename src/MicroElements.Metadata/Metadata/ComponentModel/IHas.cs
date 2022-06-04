// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Provides a knowledge that the object has a component of the declared type.
    /// Allows to get the component.
    /// Allows multiple declarations.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public interface IHas<out T>
    {
        /// <summary>
        /// Gets the component. Can return <see langword="null"/>.
        /// </summary>
        T? Component { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IHas{T}"/>.
    /// </summary>
    public static class HasExtensions
    {
        /// <summary>
        /// Gets the <see cref="IHas{T}.Component"/> value.
        /// </summary>
        /// <typeparam name="TComponent">Component type.</typeparam>
        /// <param name="source">Source.</param>
        /// <returns>The component value or <see langword="null"/>.</returns>
        public static TComponent? Component<TComponent>(this IHas<TComponent> source)
        {
            return source.Component;
        }
    }
}
