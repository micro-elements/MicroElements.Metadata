// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extension methods for <see cref="IParserProvider"/>.
    /// </summary>
    public static class ParserProviderExtensions
    {
        /// <summary>
        /// Parses dictionary to <see cref="IPropertyValue"/> list.
        /// </summary>
        /// <param name="parserProvider"><see cref="IParserProvider"/>.</param>
        /// <param name="sourceRow">Source data.</param>
        /// <returns><see cref="IPropertyValue"/> list.</returns>
        public static IReadOnlyList<IPropertyValue> ParseProperties(this IParserProvider parserProvider, IReadOnlyDictionary<string, string> sourceRow)
        {
            Option<IPropertyValue> ParseRowOrGetDefault(IPropertyParser propertyParser) =>
                sourceRow.GetValueAsOption(propertyParser.SourceName)
                    .Match(textValue => ParseUntyped(propertyParser, textValue), propertyParser.GetDefaultValueUntyped);

            var propertyValues = parserProvider.Parsers
                .Select(ParseRowOrGetDefault)
                .SelectMany(propertyValue => propertyValue)
                .ToList();

            return propertyValues;
        }

        public static Option<IPropertyValue> ParseUntyped(this IPropertyParser propertyParser, string textValue)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, string, Option<IPropertyValue>>(propertyParser.TargetType, Parse<CodeCompiler.GenericType>);
            return func(propertyParser, textValue);
        }

        public static Option<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string textValue)
        {
            var propertyParser = (IPropertyParser<T>)propertyParserUntyped;

            return propertyParser
                .ValueParser
                .Parse(textValue)
                .Map(value => (IPropertyValue)new PropertyValue<T>(propertyParser.TargetProperty, value, ValueSource.Defined));
        }
    }
}
