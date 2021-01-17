// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// ReadOnly initialized instance of <see cref="IXmlParserSettings"/>.
    /// </summary>
    public class XmlParserSettings : IXmlParserSettings
    {
        /// <inheritdoc/>
        public Func<XElement, string> GetElementName { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IParserRule> ParserRules { get; }

        /// <inheritdoc/>
        public IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <inheritdoc />
        public Func<IXmlParserSettings, IXmlParserContext> CreateContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserSettings"/> class.
        /// </summary>
        /// <param name="settings">Optional settings to build.</param>
        public XmlParserSettings(XmlParserSettingsBuilder? settings = null)
        {
            settings ??= new XmlParserSettingsBuilder();

            GetElementName = settings.GetElementName ?? XmlParser.GetElementNameDefault;
            ParserRules = settings.ParserRules ?? XmlParser.CreateDefaultXmlParsersRules().ToArray();
            PropertyComparer = settings.PropertyComparer ?? Metadata.PropertyComparer.ByReferenceComparer;
            CreateContext = settings.CreateContext ?? (parserSettings => CreateXmlParserContext(parserSettings, settings));
        }

        private static XmlParserContext CreateXmlParserContext(IXmlParserSettings parserSettings, XmlParserSettingsBuilder settings)
        {
            return new XmlParserContext(
                messages: settings.Messages ?? new MutableMessageList<Message>(),
                parsersCache: settings.ParsersCache ?? new ConcurrentDictionary<IProperty, IValueParser>(comparer: parserSettings.PropertyComparer));
        }
    }
}
