// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Cached <see cref="IParserRuleProvider"/>.
    /// </summary>
    public class CachedParserRuleProvider : IParserRuleProvider
    {
        private readonly IParserRuleProvider _parserRuleProvider;
        private readonly ConcurrentDictionary<IProperty, IParserRule> _parsersCache;
        private readonly ConcurrentDictionary<Type, IParserRule> _parsersCacheByType;

        public static CachedParserRuleProvider Create(IParserRuleProvider parserRuleProvider)
        {
            parserRuleProvider.AssertArgumentNotNull(nameof(parserRuleProvider));

            return new CachedParserRuleProvider(parserRuleProvider);
        }

        public static CachedParserRuleProvider Create(IValueParserProvider valueParserProvider)
        {
            valueParserProvider.AssertArgumentNotNull(nameof(valueParserProvider));

            var parserRules = valueParserProvider.GetValueParsers().ToParserRules().ToArray();
            var parserRuleProvider = new ParserRuleProvider(parserRules);
            return new CachedParserRuleProvider(parserRuleProvider);
        }

        public CachedParserRuleProvider(IParserRuleProvider parserRuleProvider)
        {
            parserRuleProvider.AssertArgumentNotNull(nameof(parserRuleProvider));

            _parserRuleProvider = parserRuleProvider;
            _parsersCache = new ConcurrentDictionary<IProperty, IParserRule>(comparer: parserRuleProvider.PropertyComparer);
            _parsersCacheByType = new ConcurrentDictionary<Type, IParserRule>();
        }

        /// <inheritdoc />
        public IEqualityComparer<IProperty> PropertyComparer => _parserRuleProvider.PropertyComparer;

        /// <inheritdoc />
        public IReadOnlyCollection<IParserRule> GetParserRules() => _parserRuleProvider.GetParserRules();

        /// <inheritdoc />
        public IParserRule? GetParserRule(Type type)
        {
            return _parsersCacheByType.GetOrAdd(type, (t, parserRuleProvider) => GetParserRuleInternal(parserRuleProvider, t), _parserRuleProvider);
        }

        /// <inheritdoc />
        public IParserRule? GetParserRule(IProperty property)
        {
            return _parsersCache.GetOrAdd<IParserRuleProvider>(property, (p, parserRuleProvider) => GetParserRuleInternal(parserRuleProvider, p), _parserRuleProvider);
        }

        private static IParserRule GetParserRuleInternal(IParserRuleProvider parserRuleProvider, Type type)
        {
            return parserRuleProvider.GetParserRule(type) ?? ParserRule.Empty;
        }

        private static IParserRule GetParserRuleInternal(IParserRuleProvider parserRuleProvider, IProperty property)
        {
            return parserRuleProvider.GetParserRule(property) ?? ParserRule.Empty;
        }
    }
}
