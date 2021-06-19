// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Maps type name to <see cref="Type"/> and back.
    /// </summary>
    public interface ITypeMapper
    {
        /// <summary>
        /// Gets type name.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>Type name.</returns>
        string GetTypeName(Type type);

        /// <summary>
        /// Gets type by name.
        /// It should work with all type names generated with <see cref="GetTypeName"/>.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <returns>Type.</returns>
        Type? GetTypeByName(string typeName);
    }
}
