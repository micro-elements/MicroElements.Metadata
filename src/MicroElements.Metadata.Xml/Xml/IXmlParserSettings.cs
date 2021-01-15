// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Setting for XmlParser.
    /// </summary>
    public interface IXmlParserSettings
    {
        /// <summary>
        /// Gets function that evaluates property name for xml element.
        /// </summary>
        public Func<XElement, string> GetElementName { get; }

        /// <summary>
        /// Gets parsers and rules for parsers.
        /// </summary>
        public IReadOnlyCollection<IParserRule> ParserRules { get; }

        /// <summary>
        /// Gets property comparer for property related search.
        /// Default value: <see cref="Metadata.PropertyComparer.ByReferenceComparer"/>.
        /// </summary>
        public IEqualityComparer<IProperty> PropertyComparer { get; }
    }
}
