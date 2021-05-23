// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Extensions for <see cref="IParserRuleProvider"/>.
    /// </summary>
    public static class ParserRuleProviderExtensions
    {
        public static IValueParser? GetParser(this IParserRuleProvider parserRuleProvider, Type type)
        {
            return parserRuleProvider.GetParserRule(type)?.Parser;
        }

        public static IValueParser? GetParser(this IParserRuleProvider parserRuleProvider, IProperty property)
        {
            return parserRuleProvider.GetParserRule(property)?.Parser;
        }

        public static IValueParser GetParserOrEmpty(this IParserRuleProvider parserRuleProvider, Type type)
        {
            return parserRuleProvider.GetParserRule(type)?.Parser ?? EmptyParser.Instance;
        }

        public static IValueParser GetParserOrEmpty(this IParserRuleProvider parserRuleProvider, IProperty property)
        {
            return parserRuleProvider.GetParserRule(property)?.Parser ?? EmptyParser.Instance;
        }
    }
}
