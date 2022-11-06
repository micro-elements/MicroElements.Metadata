// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents base schema interface.
    /// </summary>
    public interface ISchema :
        IMetadataProvider,
        IComposite
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
