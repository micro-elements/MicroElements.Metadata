// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.Parsing
{
    public class CollectionParser : IValueParser
    {
        private readonly IParserRuleProvider _parserRuleProvider;

        public CollectionParser(Type type, IParserRuleProvider parserRuleProvider)
        {
            Type = type;
            _parserRuleProvider = parserRuleProvider;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public IParseResult ParseUntyped(string? source)
        {
            if (source is null)
                return null;

            string[] strings = source.TrimStart('[').TrimEnd(']').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Type elementType = typeof(string);
            if (Type.IsArray)
                elementType = Type.GetElementType();

            if (elementType == typeof(string))
                return ParseResult.Success(strings);

            IParserRule? parserRule = _parserRuleProvider.GetParserRule(elementType);
            IValueParser parser = parserRule.Parser;

            var elements = strings
                .Select(s => parser.ParseUntyped(s))
                .ToArray();

            if (elements.All(result => result.IsSuccess))
            {
                var values = elements.
                    Select(result => result.ValueUntyped)
                    .ToArrayOfType(elementType);
                return new ParserResultUntyped(values.GetType(), isSuccess: true, valueUntyped: values, error: null);
            }

            var errors = elements.Where(result => result.Error != null).Select(result => result.Error).ToArray();

            return new ParseResult<object>(isSuccess: false, value: null, error: errors[0]);
        }
    }
}
