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
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<object> ParseUntyped(string source);
    }

    public interface IValueParser<T> : IValueParser
    {
        /// <summary>
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<T> Parse(string source);
    }

    public abstract class ValueParserBase<T> : IValueParser<T>
    {
        /// <inheritdoc />
        public abstract Option<T> Parse(string source);

        /// <inheritdoc />
        public Option<object> ParseUntyped(string source)
        {
            return Parse(source).MatchUntyped(Prelude.Some, () => Option<object>.None);
        }
    }

    public class ValueParser<T> : ValueParserBase<T>
    {
        private Func<string, T> _parseFunc;

        /// <inheritdoc />
        public ValueParser(Func<string, T> parseFunc)
        {
            _parseFunc = parseFunc;
        }

        /// <inheritdoc />
        public override Option<T> Parse(string source)
        {
            return _parseFunc(source);
        }
    }
}
