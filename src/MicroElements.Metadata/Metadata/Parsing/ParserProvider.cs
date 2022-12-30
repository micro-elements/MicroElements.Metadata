// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Parsing
{
    public static class ParserProvider
    {
        public static IEnumerable<IValueParser> CreateDefaultParsers() => DefaultValueParserProvider.Instance.GetValueParsers();

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
