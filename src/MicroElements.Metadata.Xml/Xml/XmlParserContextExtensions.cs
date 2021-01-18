// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using MicroElements.Functional;
using MicroElements.Metadata.Parsers;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// <see cref="IXmlParserContext"/> extensions.
    /// </summary>
    public static class XmlParserContextExtensions
    {
        /// <summary>
        /// Copies input <paramref name="context"/> with possible changes.
        /// </summary>
        /// <param name="context">Source parser context.</param>
        /// <param name="parserSettings">Optional parser settings.</param>
        /// <param name="messages">Optional message list.</param>
        /// <param name="parsersCache">Optional parsers cache.</param>
        /// <param name="schemaCache">Optional schemas cache.</param>
        /// <returns>New <see cref="IXmlParserContext"/> instance.</returns>
        public static IXmlParserContext With(
            this IXmlParserContext context,
            IXmlParserSettings? parserSettings = null,
            IMutableMessageList<Message>? messages = null,
            ConcurrentDictionary<IProperty, IValueParser>? parsersCache = null,
            ConcurrentDictionary<IProperty, ISchema>? schemaCache = null)
        {
            context.AssertArgumentNotNull(nameof(context));

            return new XmlParserContext(
                parserSettings: parserSettings ?? context.ParserSettings,
                messages: messages ?? context.Messages,
                parsersCache: parsersCache ?? context.ParsersCache,
                schemaCache: schemaCache ?? context.SchemaCache);
        }

        /// <summary>
        /// Gets parser from context cache or <see cref="IXmlParserSettings.ParserRules"/>.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <returns>Parser.</returns>
        public static IValueParser GetParserCached(this IXmlParserContext context, IProperty property)
        {
            return context.ParsersCache.GetOrAdd(property, p => context.ParserSettings.ParserRules.GetParserRule(p, context.ParserSettings.PropertyComparer)?.Parser ?? EmptyParser.Instance);
        }

        /// <summary>
        /// Gets or adds schema to property.
        /// If property is null then returns new empty schema.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <param name="factory">Schema factory.</param>
        /// <returns>Schema attached to property.</returns>
        public static ISchema GetOrAddSchema(this IXmlParserContext context, IProperty? property, Func<ISchema>? factory = null)
        {
            if (property == null)
                return factory?.Invoke() ?? new MutableSchema();

            return context.SchemaCache.GetOrAdd(property, p => p.GetOrAddSchema(factory));
        }
    }
}
