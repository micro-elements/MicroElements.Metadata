// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents base schema interface.
    /// </summary>
    public interface ISchema : IMetadataProvider
    {
        /// <summary>
        /// Gets property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets property value type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets property description.
        /// </summary>
        string? Description { get; }
    }

    /// <summary>
    /// Represents base strong typed schema interface.
    /// </summary>
    public interface ISchema<T> : ISchema
    {
    }
}
