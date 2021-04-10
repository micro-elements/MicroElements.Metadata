// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Methods for building <see cref="IPropertyParser"/>.
    /// </summary>
    public static class PropertyParser
    {
        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and <paramref name="valueParser"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="sourceName">Source name.</param>
        /// <param name="valueParser">Value parser.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public static PropertyParser<T> Source<T>(string sourceName, IValueParser<T> valueParser)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, valueParser, null);
            return propertyParser;
        }

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and parse func <paramref name="parseFunc"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="sourceName">Source name.</param>
        /// <param name="parseFunc">Parse value function.</param>
        /// <returns><see cref="PropertyParser{T}"/>.</returns>
        public static PropertyParser<T> Source<T>(string sourceName, Func<string, T> parseFunc)
        {
            PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, new ValueParser<T>(parseFunc), null);
            return propertyParser;
        }

        //TODO: Migrate
        ///// <summary>
        ///// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> and parse func <paramref name="parseFunc"/>.
        ///// </summary>
        ///// <typeparam name="T">Property type.</typeparam>
        ///// <param name="sourceName">Source name.</param>
        ///// <param name="parseFunc">Parse value function.</param>
        ///// <returns><see cref="PropertyParser{T}"/>.</returns>
        //public static PropertyParser<T> Source<T>(string sourceName, Func<string, Option<T>> parseFunc)
        //{
        //    PropertyParser<T> propertyParser = new PropertyParser<T>(sourceName, new ValueParser<T>(parseFunc), null);
        //    return propertyParser;
        //}

        /// <summary>
        /// Adds new <see cref="PropertyParser{T}"/> with <paramref name="sourceName"/> .
        /// </summary>
        /// <param name="sourceName">Source name.</param>
        /// <returns><see cref="PropertyParser{T}"/> of string.</returns>
        public static PropertyParser<string> Source(string sourceName)
        {
            PropertyParser<string> propertyParser = new PropertyParser<string>(sourceName, StringParser.Default, null);
            return propertyParser;
        }
    }
}
