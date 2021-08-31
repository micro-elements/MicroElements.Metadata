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
        /// Returns only success results.
        /// </summary>
        /// <param name="parserProvider"><see cref="IParserProvider"/>.</param>
        /// <param name="sourceRow">Source data.</param>
        /// <returns><see cref="IPropertyValue"/> list.</returns>
        public static IReadOnlyList<IPropertyValue> ParseProperties(this IParserProvider parserProvider, IReadOnlyDictionary<string, string> sourceRow)
        {
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));
            sourceRow.AssertArgumentNotNull(nameof(sourceRow));

            var propertyValues = parserProvider
                .GetParsers()
                .Select(parser => parser.ParsePropertyUntyped(sourceRow))
                .Where(result => result.IsSuccess && result.Value is not null)
                .Select(result => result.Value!)
                .ToList();

            return propertyValues;
        }

        /// <summary>
        /// Parses dictionary to <see cref="ParseResult{IPropertyValue}"/> list.
        /// </summary>
        /// <param name="parserProvider"><see cref="IParserProvider"/>.</param>
        /// <param name="sourceRow">Source data.</param>
        /// <returns><see cref="IPropertyValue"/> list.</returns>
        public static IReadOnlyList<ParseResult<IPropertyValue>> ParsePropertiesAsParseResults(this IParserProvider parserProvider, IReadOnlyDictionary<string, string> sourceRow)
        {
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));
            sourceRow.AssertArgumentNotNull(nameof(sourceRow));

            List<ParseResult<IPropertyValue>> parseResults = parserProvider
                .GetParsers()
                .Select(parser => parser.ParsePropertyUntyped(sourceRow))
                .ToList();

            return parseResults;
        }
    }
}
