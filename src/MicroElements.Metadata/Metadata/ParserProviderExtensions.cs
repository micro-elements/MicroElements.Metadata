// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

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
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));
            sourceRow.AssertArgumentNotNull(nameof(sourceRow));

            var propertyValues = parserProvider
                .GetParsers()
                .Select(parser => parser.ParseRowUntyped(sourceRow))
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .ToList();

            return propertyValues;
        }
    }
}
