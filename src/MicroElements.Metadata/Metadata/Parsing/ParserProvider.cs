// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Parsing
{
    public static class ParserProvider
    {
        public static IEnumerable<IValueParser> CreateDefaultParsers()
        {
            // String.
            yield return StringParser.Default;

            // Numeric types.
            yield return Parser.ByteParser;
            yield return Parser.ShortParser;
            yield return Parser.IntParser;
            yield return Parser.LongParser;
            yield return Parser.SByteParser;
            yield return Parser.UShortParser;
            yield return Parser.UIntParser;
            yield return Parser.ULongParser;
            yield return Parser.FloatParser;
            yield return Parser.DoubleParser;
            yield return Parser.DecimalParser;

            // Nullable numeric types.
            yield return Parser.NullableByteParser;
            yield return Parser.NullableShortParser;
            yield return Parser.NullableIntParser;
            yield return Parser.NullableLongParser;
            yield return Parser.NullableSByteParser;
            yield return Parser.NullableUShortParser;
            yield return Parser.NullableUIntParser;
            yield return Parser.NullableULongParser;
            yield return Parser.NullableFloatParser;
            yield return Parser.NullableDoubleParser;
            yield return Parser.NullableDecimalParser;

            // Bool.
            yield return Parser.BoolParser;
            yield return Parser.NullableBoolParser;

            // DateTime.
            yield return Parser.DateTimeParser;
            yield return Parser.NullableDateTimeParser;
        }

        public static IEnumerable<IParserRule> ToParserRules(this IEnumerable<IValueParser> parsers)
        {
            foreach (IValueParser valueParser in parsers)
            {
                yield return new ParserRule(valueParser, valueParser.Type);
            }
        }

        public static IEnumerable<IParserRule> CreateDefaultXmlParsersRules()
        {
            return CreateDefaultParsers().ToParserRules();
        }
    }
}
