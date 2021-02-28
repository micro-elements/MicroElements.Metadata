// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Factory for <see cref="IProperty"/>.
    /// </summary>
    public interface IPropertyFactory
    {
        /// <summary>
        /// Creates new property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
        IProperty<T> Create<T>(string name);

        /// <summary>
        /// Creates property by <paramref name="type"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Property type.</param>
        /// <param name="name">Property name.</param>
        /// <returns>Created property.</returns>
        IProperty Create(Type type, string name);
    }
}
