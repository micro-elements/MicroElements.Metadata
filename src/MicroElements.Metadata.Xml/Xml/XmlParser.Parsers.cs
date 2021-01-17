// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using MicroElements.Functional;
using MicroElements.Metadata.Parsers;

namespace MicroElements.Metadata.Xml
{
    public static partial class XmlParser
    {
        public static IValueParser GetParserCached(IXmlParserSettings settings, IXmlParserContext context, IProperty property)
        {
            return context.ParsersCache.GetOrAdd(property, p => settings.ParserRules.GetParserRule(p, settings.PropertyComparer)?.Parser ?? EmptyParser.Instance);
        }

        public static IEnumerable<IValueParser> CreateDefaultXmlParsers()
        {
            yield return StringParser.Instance;

            yield return new ValueParser<int>(ParseInt);
            yield return new ValueParser<double>(ParseDouble);
            yield return new ValueParser<DateTime>(ParseDateTime);

            yield return new ValueParser<int?>(ParseNullableInt);
            yield return new ValueParser<double?>(ParseNullableDouble);
            yield return new ValueParser<DateTime?>(ParseNullableDateTime);

            yield return new ValueParser<bool>(ParseBool);
            yield return new ValueParser<bool?>(ParseNullableBool);

            yield return new ValueParser<decimal>(ParseDecimal);
            yield return new ValueParser<decimal?>(ParseNullableDecimal);
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
            return CreateDefaultXmlParsers().ToParserRules();
        }

        public static Option<T?> ToNullable<T>(this Option<T> optional)
            where T : struct
            => optional.MatchUnsafe(value => value, default(T?));

        public static Option<bool> ParseBool(string text) => Prelude.ParseBool(text);

        public static Option<bool?> ParseNullableBool(string text) => ParseBool(text).ToNullable();

        public static Option<int> ParseInt(string text) => Prelude.ParseInt(text);

        public static Option<int?> ParseNullableInt(string text) => ParseInt(text).ToNullable();

        public static Option<double> ParseDouble(string text) => Prelude.ParseDouble(text, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static Option<double?> ParseNullableDouble(string text) => ParseDouble(text).ToNullable();

        public static Option<decimal> ParseDecimal(string text) => decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : Option<decimal>.None;

        public static Option<decimal?> ParseNullableDecimal(string text) => ParseDecimal(text).ToNullable();

        public static Option<DateTime> ParseDateTime(string text)
        {
            var parseResult = DateTime.TryParse(text, out DateTime result);
            return parseResult ? result : Option<DateTime>.None;
        }

        public static Option<DateTime?> ParseNullableDateTime(string text) => ParseDateTime(text).ToNullable();
    }
}
