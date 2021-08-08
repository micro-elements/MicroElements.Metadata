// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Null type (In terms of JsonSchema).
    /// Represents schema that can accept null value.
    /// </summary>
    public class NullTypeSchema : ISchema, IReadOnly
    {
        /// <inheritdoc />
        public string Name => "null";

        /// <inheritdoc />
        public Type Type => typeof(object);

        /// <inheritdoc />
        public string? Description => "Null type.";
    }
}
