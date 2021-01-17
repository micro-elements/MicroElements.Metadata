// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Empty parser is NullObject for <see cref="IValueParser"/>.
    /// It parses any string to <see cref="OptionNone"/>.
    /// </summary>
    public sealed class EmptyParser : IValueParser
    {
        /// <summary>
        /// Global static instance.
        /// </summary>
        public static readonly EmptyParser Instance = new EmptyParser();

        /// <inheritdoc />
        public Type Type => typeof(object);

        /// <inheritdoc />
        public Option<object> ParseUntyped(string source) => Option<object>.None;

        private EmptyParser()
        {
        }
    }
}
