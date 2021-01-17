// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MicroElements.Functional;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public interface IXmlParserContext
    {
        /// <summary>
        /// Gets messages list.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }

        /// <summary>
        /// Gets parsers cache.
        /// </summary>
        ConcurrentDictionary<IProperty, IValueParser> ParsersCache { get; }
    }

    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public class XmlParserContext : IXmlParserContext
    {
        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<IProperty, IValueParser> ParsersCache { get; }

        public XmlParserContext(
            IMutableMessageList<Message>? messages = null,
            ConcurrentDictionary<IProperty, IValueParser>? parsersCache = null)
        {
            Messages = messages ?? new MutableMessageList<Message>();
            ParsersCache = parsersCache ?? new ConcurrentDictionary<IProperty, IValueParser>(comparer: PropertyComparer.ByReferenceComparer);
        }
    }
}
