// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property parsers.
    /// </summary>
    public interface IParserProvider
    {
        /// <summary>
        /// Gets property parsers.
        /// </summary>
        /// <returns>Enumerable of <see cref="IPropertyParser"/>.</returns>
        IEnumerable<IPropertyParser> GetParsers();
    }

    /// <summary>
    /// Provides value parsers.
    /// Value parsers knows only how to parse text to some type.
    /// For more high-level parsing use <see cref="IParserRuleProvider"/>.
    /// </summary>
    public interface IValueParserProvider
    {
        /// <summary>
        /// Gets value parsers.
        /// </summary>
        /// <returns>Enumerable of <see cref="IValueParser"/>.</returns>
        IEnumerable<IValueParser> GetValueParsers();
    }

    /// <summary>
    /// Provides parsers rules.
    /// Knows how to match source and target type or property.
    /// </summary>
    public interface IParserRuleProvider
    {
        /// <summary>
        /// Gets parser rules.
        /// </summary>
        /// <returns>Enumerable of <see cref="IParserRule"/>.</returns>
        IEnumerable<IParserRule> GetParserRules();
    }

    public class CachedPropertyParserProvider
    {
        private readonly IReadOnlyCollection<IParserRule> _parserRules;
        private readonly IEqualityComparer<IProperty> _propertyComparer;
        private readonly ConcurrentDictionary<IProperty, IValueParser> _parsersCache;

        public static CachedPropertyParserProvider Create(
            IParserRuleProvider parserRuleProvider,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            parserRuleProvider.AssertArgumentNotNull(nameof(parserRuleProvider));

            var parserRules = parserRuleProvider.GetParserRules().ToArray();
            return new CachedPropertyParserProvider(parserRules, propertyComparer);
        }

        public static CachedPropertyParserProvider Create(
            IValueParserProvider valueParserProvider,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            valueParserProvider.AssertArgumentNotNull(nameof(valueParserProvider));

            var parserRules = valueParserProvider.GetValueParsers().ToParserRules().ToArray();
            return new CachedPropertyParserProvider(parserRules, propertyComparer);
        }

        public CachedPropertyParserProvider(
            IReadOnlyCollection<IParserRule> parserRules,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            parserRules.AssertArgumentNotNull(nameof(parserRules));

            _parserRules = parserRules;
            _propertyComparer = propertyComparer ?? PropertyComparer.ByTypeAndNameComparer;
            _parsersCache = new ConcurrentDictionary<IProperty, IValueParser>(comparer: _propertyComparer);
        }


        /// <summary>
        /// Gets parser from context cache or <see cref="IXmlParserSettings.ParserRules"/>.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <returns>Parser.</returns>
        public IValueParser GetParser(IProperty property)
        {
            if (_parsersCache.TryGetValue(property, out IValueParser result))
                return result;

            result = _parserRules.GetParserRule(property, _propertyComparer)?.Parser ?? EmptyParser.Instance;
            _parsersCache.TryAdd(property, result);

            return result;
        }
    }
}
