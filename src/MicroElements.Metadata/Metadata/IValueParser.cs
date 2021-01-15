// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Text value parser.
    /// </summary>
    public interface IValueParser
    {
        /// <summary>
        /// Gets the target type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<object> ParseUntyped(string source);
    }

    /// <summary>
    /// Typed value parser.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IValueParser<T> : IValueParser
    {
        /// <summary>
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<T> Parse(string source);
    }

    /// <summary>
    /// Value parser base class.
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
