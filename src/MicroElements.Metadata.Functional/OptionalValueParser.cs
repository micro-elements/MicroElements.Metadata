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
    public class OptionalValueParser<T> : ValueParserBase<T>
    {
        private readonly Func<string, Option<T>>? _parseFunc2;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalValueParser{T}"/> class.
        /// </summary>
        /// <param name="parseFunc">Func to parse string to value.</param>
        public OptionalValueParser(Func<string?, Option<T>> parseFunc)
        {
            parseFunc.AssertArgumentNotNull(nameof(parseFunc));

            _parseFunc2 = parseFunc;
        }

        /// <inheritdoc />
        public override IParseResult<T> Parse(string? source)
        {
            try
            {
                Option<T> optionalValue = _parseFunc2.Invoke(source);
                if (optionalValue.IsSome)
                    return ParseResult.Success((T)optionalValue);

                return ParseResult.Cache<T>.Failed;
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
        }
    }
}
