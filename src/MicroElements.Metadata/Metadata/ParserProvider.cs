// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

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

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and <paramref name="valueParser"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="sourceName">Source name.</param>
        /// <param name="valueParser">Value parser.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public PropertyParser<T> Source<T>(string sourceName, IValueParser<T> valueParser)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, valueParser, null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and parse func <paramref name="parseFunc"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="sourceName">Source name.</param>
        /// <param name="parseFunc">Parse value function.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public PropertyParser<T> Source<T>(string sourceName, Func<string, T> parseFunc)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, new ValueParser<T>(parseFunc), null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and parse func <paramref name="parseFunc"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="sourceName">Source name.</param>
        /// <param name="parseFunc">Parse value function.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public PropertyParser<T> Source<T>(string sourceName, Func<string, Option<T>> parseFunc)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, new ValueParser<T>(parseFunc), null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> .
        /// </summary>
        /// <param name="sourceName">Source name.</param>
        /// <returns><see cref="PropertyParser{T}"/> of string.</returns>
        public PropertyParser<string> Source(string sourceName)
        {
            PropertyParser<string> propertyParser = new PropertyParser<string>(sourceName, StringParser.Instance, null);
            _parsers.Add(propertyParser);
            return propertyParser;
        }

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with target property <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public PropertyParser<T> Target<T>(IProperty<T> property)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(null, null, property);
            _parsers.Add(propertyParser);
            return propertyParser;
        }
    }
}
