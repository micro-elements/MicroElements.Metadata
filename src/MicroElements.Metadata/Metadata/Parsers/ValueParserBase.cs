// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Generic value parser base class.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public abstract class ValueParserBase<T> : IValueParser<T>
    {
        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public abstract Option<T> Parse(string source);

        /// <inheritdoc />
        public Option<object> ParseUntyped(string source)
        {
            return Parse(source).MatchUntyped(Prelude.Some, () => Option<object>.None);
        }
    }
}
