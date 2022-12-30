// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Diagnostics;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Empty parser is NullObject for <see cref="IValueParser"/>.
    /// It parses any string to <see cref=""/>.
    /// </summary>
    public sealed class EmptyParser : IValueParser
    {
        /// <summary>
        /// Global static instance.
        /// </summary>
        public static EmptyParser Instance { get; } = new EmptyParser();

        /// <inheritdoc />
        public Type Type => typeof(object);

        /// <inheritdoc />
        public IParseResult ParseUntyped(string? source) => ParseResult.Failed<object>(new Message($"EmptyParser does not know how to parse value {source}"));

        private EmptyParser()
        {
        }
    }
}
