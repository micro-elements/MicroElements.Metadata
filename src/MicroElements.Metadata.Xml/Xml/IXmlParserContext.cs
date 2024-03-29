﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MicroElements.CodeContracts;
using MicroElements.Diagnostics;
using MicroElements.Metadata.Parsing;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public interface IXmlParserContext
    {
        /// <summary>
        /// Gets <see cref="IXmlParserSettings"/> for this context.
        /// </summary>
        IXmlParserSettings ParserSettings { get; }

        /// <summary>
        /// Gets schema to use for parsing.
        /// </summary>
        ISchema Schema { get; }

        /// <summary>
        /// Gets messages list.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }

        /// <summary>
        /// Gets cached parser provider.
        /// </summary>
        IParserRuleProvider ParserRuleProvider { get; }

        /// <summary>
        /// Gets schema cache.
        /// </summary>
        ConcurrentDictionary<IProperty, ISchema> SchemaCache { get; }

        /// <summary>
        /// Gets validators cache.
        /// </summary>
        ConcurrentDictionary<IProperty, IPropertyValidationRules> ValidatorsCache { get; }
    }

    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public class XmlParserContext : IXmlParserContext
    {
        /// <inheritdoc />
        public IXmlParserSettings ParserSettings { get; }

        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public IParserRuleProvider ParserRuleProvider { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<IProperty, ISchema> SchemaCache { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<IProperty, IPropertyValidationRules> ValidatorsCache { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserContext"/> class.
        /// </summary>
        /// <param name="parserSettings">Parser settings.</param>
        /// <param name="schema">Schema to use for parsing.</param>
        /// <param name="messages">Optional message list.</param>
        /// <param name="parserRuleProvider">Optional parser rule provider.</param>
        /// <param name="schemaCache">Optional schemas cache.</param>
        /// <param name="validatorsCache">Optional validators cache.</param>
        public XmlParserContext(
            IXmlParserSettings parserSettings,
            ISchema? schema,
            IMutableMessageList<Message>? messages = null,
            IParserRuleProvider? parserRuleProvider = null,
            ConcurrentDictionary<IProperty, ISchema>? schemaCache = null,
            ConcurrentDictionary<IProperty, IPropertyValidationRules>? validatorsCache = null)
        {
            parserSettings.AssertArgumentNotNull(nameof(parserSettings));

            ParserSettings = parserSettings;
            Schema = schema ?? new MutableObjectSchema();

            Messages = messages ?? new MutableMessageList<Message>();

            SchemaCache = schemaCache ?? new ConcurrentDictionary<IProperty, ISchema>(comparer: parserSettings.PropertyComparer);
            ValidatorsCache = validatorsCache ?? new ConcurrentDictionary<IProperty, IPropertyValidationRules>(comparer: parserSettings.PropertyComparer);

            if (parserRuleProvider != null )
            {
                if (parserRuleProvider is CachedParserRuleProvider)
                    ParserRuleProvider = parserRuleProvider;
                else
                    ParserRuleProvider = new CachedParserRuleProvider(parserRuleProvider);
            }

            else
            {
                var ruleProvider = new ParserRuleProvider(parserSettings.ParserRules, parserSettings.PropertyComparer);
                ParserRuleProvider = new CachedParserRuleProvider(ruleProvider);
            }
        }
    }
}
