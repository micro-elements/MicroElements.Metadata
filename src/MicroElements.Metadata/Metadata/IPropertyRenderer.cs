// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent property renderer.
    /// </summary>
    public interface IPropertyRenderer : IMetadataProvider
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        IProperty PropertyUntyped { get; }

        /// <summary>
        /// Gets property type.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets target name.
        /// </summary>
        string TargetName { get; }

        /// <summary>
        /// Method to render property as text value.
        /// </summary>
        /// <param name="source">Source object to render.</param>
        /// <returns>Rendered text value.</returns>
        string Render(IPropertyContainer source);
    }

    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyRenderer<out T> : IPropertyRenderer
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        IProperty<T> Property { get; }
    }
}
