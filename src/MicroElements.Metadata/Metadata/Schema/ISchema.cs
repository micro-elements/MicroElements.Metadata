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
        /// Gets name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets value type.
        /// </summary>
        Type Type { get; }
    }

    /// <summary>
    /// Represents base strong typed schema interface.
    /// </summary>
    public interface ISchema<out T> : ISchema
    {
    }
}
