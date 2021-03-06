﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.JsonSchema;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// https://json-schema.org/understanding-json-schema/reference/array.html#array
    /// </summary>
    public class ArraySchema : ISchema
    {
        public ISchema Items { get; set; }

        /// <inheritdoc />
        public string Name => JsonSimpleType.Array.Name;

        /// <inheritdoc />
        public Type Type { get; set; }

        /// <inheritdoc />
        public string? Description { get; set; }
    }
}
