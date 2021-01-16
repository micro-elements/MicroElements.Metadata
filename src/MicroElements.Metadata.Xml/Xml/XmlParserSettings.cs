// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Mutable setting for XmlParser.
    /// </summary>
    public class XmlParserSettings
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
        /// Builds filled and valid <see cref="IXmlParserSettings"/>.
        /// </summary>
        /// <returns><see cref="IXmlParserSettings"/> instance.</returns>
        public IXmlParserSettings Build() => new ReadOnlyXmlParserSettings(this);
    }

    /// <summary>
    /// ReadOnly builded version of <see cref="XmlParserSettings"/>.
    /// </summary>
    public class ReadOnlyXmlParserSettings : IXmlParserSettings
    {
        /// <inheritdoc/>
        public Func<XElement, string> GetElementName { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IParserRule> ParserRules { get; }

        /// <inheritdoc/>
        public IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyXmlParserSettings"/> class.
        /// </summary>
        /// <param name="settings">Optional settings to build.</param>
        public ReadOnlyXmlParserSettings(XmlParserSettings? settings)
        {
            settings ??= new XmlParserSettings();

            GetElementName = settings.GetElementName ?? XmlParser.GetElementNameDefault;
            ParserRules = settings.ParserRules ?? XmlParser.CreateDefaultXmlParsersRules().ToArray();
            PropertyComparer = settings.PropertyComparer ?? Metadata.PropertyComparer.ByReferenceComparer;
            Messages = settings.Messages ?? new MutableMessageList<Message>();
        }
    }
}
