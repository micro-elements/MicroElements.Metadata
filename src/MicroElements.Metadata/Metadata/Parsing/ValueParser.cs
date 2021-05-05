// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;
using Message = MicroElements.Diagnostics.Message;
using MessageSeverity = MicroElements.Diagnostics.MessageSeverity;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Value parser implementation with external func.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class ValueParser<T> : ValueParserBase<T>
    {
        private readonly Func<string, T>? _parseFunc1;
        private readonly Func<string, Option<T>>? _parseFunc2;
        private readonly Func<string, ParseResult<T>>? _parseFunc3;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueParser{T}"/> class.
        /// </summary>
        /// <param name="parseFunc">Func to parse string to value.</param>
        public ValueParser(Func<string?, T> parseFunc)
        {
            _parseFunc1 = parseFunc;
        }

        //TODO: Migrate
        ///// <summary>
        ///// Initializes a new instance of the <see cref="ValueParser{T}"/> class.
        ///// </summary>
        ///// <param name="parseFunc">Func to parse string to value.</param>
        //public ValueParser(Func<string?, Option<T>> parseFunc)
        //{
        //    _parseFunc2 = parseFunc;
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueParser{T}"/> class.
        /// </summary>
        /// <param name="parseFunc">Func to parse string to value.</param>
        public ValueParser(Func<string?, ParseResult<T>> parseFunc)
        {
            _parseFunc3 = parseFunc;
        }

        /// <inheritdoc />
        public override IParseResult<T> Parse(string? source)
        {
            try
            {
                if (_parseFunc1 != null)
                {
                    T value = _parseFunc1(source);
                    return ParseResult.Success(value);
                }

                //TODO: Migrate
                //if (_parseFunc2 != null)
                //{
                //    Option<T> optionalValue = _parseFunc2.Invoke(source);
                //    return optionalValue.ToParseResult();
                //}

                if (_parseFunc3 != null)
                {
                    ParseResult<T> parseResult = _parseFunc3.Invoke(source);
                    return parseResult;
                }
            }
            catch (Exception e)
            {
                return ParseResult.Failed<T>(new Message(
                    "Failed to parse value '{value}' to type '{type}'. Error: {error}",
                    severity: MessageSeverity.Error,
                    eventName: "ParserError",
                    properties: new[]
                    {
                        new KeyValuePair<string, object>("value", source),
                        new KeyValuePair<string, object>("type", typeof(T)),
                        new KeyValuePair<string, object>("error", e.Message),
                    }));
            }

            return ParseResult.Failed<T>();
        }
    }
}
