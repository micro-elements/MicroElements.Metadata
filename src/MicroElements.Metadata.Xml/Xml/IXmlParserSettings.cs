// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MicroElements.Validation.Rules;

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
        Func<XElement, string> GetElementName { get; }

        /// <summary>
        /// Gets parsers and rules for parsers.
        /// </summary>
        IReadOnlyCollection<IParserRule> ParserRules { get; }

        /// <summary>
        /// Gets property comparer for property related search.
        /// Default value: <see cref="Metadata.PropertyComparer.ByReferenceComparer"/>.
        /// </summary>
        IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <summary>
        /// Gets factory for property values.
        /// </summary>
        IPropertyValueFactory PropertyValueFactory { get; }

        /// <summary>
        /// Gets string provider to support string interning or caching for performance needs.
        /// </summary>
        IStringProvider StringProvider { get; }

        /// <summary>
        /// Gets validation factory.
        /// </summary>
        IValidationProvider ValidationProvider { get; }

        /// <summary>
        /// Gets a value indicating whether property value should be validated on parse.
        /// </summary>
        bool ValidateOnParse { get; }

        /// <summary>
        /// Gets a value indicating whether schema should be set for every object.
        /// </summary>
        bool SetSchemaForObjects { get; }
    }
}
