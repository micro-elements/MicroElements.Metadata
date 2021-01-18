// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
