// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Value parser implementation with external func.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class ValueParser<T> : ValueParserBase<T>
    {
        private readonly Func<string, T>? _parseFuncSimple;
        private readonly Func<string, Option<T>>? _parseFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueParser{T}"/> class.
        /// </summary>
        /// <param name="parseFunc">Func to parse string to value.</param>
        public ValueParser(Func<string, T> parseFunc)
        {
            _parseFuncSimple = parseFunc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueParser{T}"/> class.
        /// </summary>
        /// <param name="parseFunc">Func to parse string to value.</param>
        public ValueParser(Func<string, Option<T>> parseFunc)
        {
            _parseFunc = parseFunc;
        }

        /// <inheritdoc />
        public override Option<T> Parse(string source)
        {
            return _parseFuncSimple != null ? _parseFuncSimple(source) : _parseFunc?.Invoke(source) ?? Option<T>.None;
        }
    }
}
