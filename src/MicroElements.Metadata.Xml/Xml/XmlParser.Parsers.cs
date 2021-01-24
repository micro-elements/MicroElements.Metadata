// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata.Xml
{
    public static partial class XmlParser
    {
        public static IEnumerable<IValueParser> CreateDefaultXmlParsers()
        {
            return Parsing.ParserProvider.CreateDefaultParsers();
        }

        public static IEnumerable<IParserRule> CreateDefaultXmlParsersRules()
        {
            return CreateDefaultXmlParsers().ToParserRules();
        }
    }
}
