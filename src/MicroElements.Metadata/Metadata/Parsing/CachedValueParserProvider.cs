// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata
{
    public class CachedValueParserProvider
    {
        private readonly IReadOnlyCollection<IParserRule> _parserRules;
        private readonly IEqualityComparer<IProperty> _propertyComparer;
        private readonly ConcurrentDictionary<IProperty, IValueParser> _parsersCache;

        public static CachedValueParserProvider Create(
            IParserRuleProvider parserRuleProvider,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            parserRuleProvider.AssertArgumentNotNull(nameof(parserRuleProvider));

            var parserRules = parserRuleProvider.GetParserRules().ToArray();
            return new CachedValueParserProvider(parserRules, propertyComparer);
        }

        public static CachedValueParserProvider Create(
            IValueParserProvider valueParserProvider,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            valueParserProvider.AssertArgumentNotNull(nameof(valueParserProvider));

            var parserRules = valueParserProvider.GetValueParsers().ToParserRules().ToArray();
            return new CachedValueParserProvider(parserRules, propertyComparer);
        }

        public CachedValueParserProvider(
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
        /// TODO: replace GetParserCached
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Parser.</returns>
        public IValueParser GetParser(IProperty property)
        {
            return _parsersCache.GetOrAdd(property, GetParserInternal);
        }

        private IValueParser GetParserInternal(IProperty property)
        {
            var parserRule = GetParserRule(property);
            return parserRule?.Parser ?? EmptyParser.Instance;
        }

        private IParserRule? GetParserRule(IProperty property)
        {
            return _parserRules.GetParserRule(property, _propertyComparer);
        }
    }
}
