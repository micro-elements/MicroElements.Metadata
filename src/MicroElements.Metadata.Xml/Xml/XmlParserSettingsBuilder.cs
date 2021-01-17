// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Mutable setting for XmlParser.
    /// </summary>
    public class XmlParserSettingsBuilder
    {
        /// <summary>
        /// Gets or sets function that evaluates property name for xml element.
        /// </summary>
        public Func<XElement, string>? GetElementName { get; set; }

        /// <summary>
        /// Parsers and rules for parsers.
        /// </summary>
        public IReadOnlyCollection<IParserRule>? ParserRules { get; set; }

        /// <summary>
        /// Optional property comparer for property related search.
        /// </summary>
        public IEqualityComparer<IProperty>? PropertyComparer { get; set; }

        /// <summary>
        /// Gets messages list.
        /// </summary>
        public IMutableMessageList<Message>? Messages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentDictionary<IProperty, IValueParser>? ParsersCache { get; set; }

        /// <summary>
        /// Gets or sets create context function.
        /// </summary>
        public Func<IXmlParserSettings, IXmlParserContext>? CreateContext { get; set; }

        public IXmlParserSettings Build()
        {
            return new XmlParserSettings(this);
        }
    }
}
