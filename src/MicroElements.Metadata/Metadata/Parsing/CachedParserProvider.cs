// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Cached parser provider.
    /// Use to cache lazy parser providers.
    /// </summary>
    public class CachedParserProvider : IParserProvider
    {
        private readonly IReadOnlyCollection<IPropertyParser> _parsers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedParserProvider"/> class.
        /// </summary>
        /// <param name="parserProvider">Parser provider to cache.</param>
        public CachedParserProvider(IParserProvider parserProvider)
        {
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            _parsers = parserProvider.GetParsers().ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedParserProvider"/> class.
        /// </summary>
        /// <param name="parsers">Parsers to cache.</param>
        public CachedParserProvider(IEnumerable<IPropertyParser> parsers)
        {
            parsers.AssertArgumentNotNull(nameof(parsers));

            _parsers = parsers.ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<IPropertyParser> GetParsers() => _parsers;
    }

    /// <summary>
    /// <see cref="CachedParserProvider"/> extensions.
    /// </summary>
    public static class CachedParserProviderExtensions
    {
        /// <summary>
        /// Creates cached parser provider (enumerates and caches parsers).
        /// </summary>
        /// <param name="parserProvider">Parser provider to cache.</param>
        /// <returns>Cached parser provider.</returns>
        public static IParserProvider Cached(this IParserProvider parserProvider)
        {
            return new CachedParserProvider(parserProvider);
        }

        /// <summary>
        /// Creates cached parser provider (enumerates and caches parsers).
        /// </summary>
        /// <param name="parsers">Parsers to cache.</param>
        /// <returns>Cached parser provider.</returns>
        public static IParserProvider Cached(this IEnumerable<IPropertyParser> parsers)
        {
            return new CachedParserProvider(parsers);
        }
    }
}
