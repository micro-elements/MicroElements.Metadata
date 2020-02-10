// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Base <see cref="IParserProvider"/> implementation with builder support.
    /// </summary>
    public abstract class ParserProvider : IParserProvider
    {
        private readonly List<IPropertyParser> _parsers = new List<IPropertyParser>();

        /// <inheritdoc />
        public IEnumerable<IPropertyParser> Parsers => _parsers;

        public PropertyParser<T> Source<T>(string sourceName, IValueParser<T> valueParser)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, valueParser, null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        public PropertyParser<T> Source<T>(string sourceName, Func<string, T> parseFunc)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, new ValueParser<T>(parseFunc), null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        public PropertyParser<string> Source(string sourceName)
        {
            PropertyParser<string> propertyParser = new PropertyParser<string>(sourceName, StringParser.Instance, null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        public PropertyParser<T> Target<T>(IProperty<T> property)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(null, null, property);
            _parsers.Add(propertyParser);
            return propertyParser;
        }
    }
}
