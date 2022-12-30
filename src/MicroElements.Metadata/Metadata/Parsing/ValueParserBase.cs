// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Generic value parser base class.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public abstract class ValueParserBase<T> : IValueParser<T>
    {
        /// <summary>
        /// Gets target value type.
        /// </summary>
        public Type Type => typeof(T);

        /// <inheritdoc />
        public abstract IParseResult<T> Parse(string? source);

        /// <inheritdoc />
        public IParseResult ParseUntyped(string? source) => Parse(source);
    }
}
