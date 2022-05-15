// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides a knowledge that the object has a component of the declared type.
    /// Allows to get the component.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public interface IHas<out T>
    {
        /// <summary>
        /// Gets the component. Can return <see langword="null"/>.
        /// </summary>
        T? Component { get; }
    }
}
