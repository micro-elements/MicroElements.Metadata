﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MicroElements.Metadata.Parsing;
using MicroElements.Validation.Rules;

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
        public IPropertyFactory PropertyFactory { get; }

        /// <inheritdoc />
        public IPropertyValueFactory PropertyValueFactory { get; }

        /// <inheritdoc />
        public IStringProvider StringProvider { get; }

        /// <inheritdoc />
        public IValidationProvider ValidationProvider { get; }

        /// <inheritdoc />
        public bool ValidateOnParse { get; }

        /// <inheritdoc />
        public bool SetSchemaForObjects { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserSettings"/> class.
        /// </summary>
        /// <param name="getElementName">Function that evaluates property name for xml element.</param>
        /// <param name="parserRules">Parsers and rules for parsers.</param>
        /// <param name="stringProvider">Optional string provider.</param>
        /// <param name="propertyComparer">Property comparer for property related search. Default value: <see cref="Metadata.PropertyComparer.ByReferenceComparer"/>.</param>
        /// <param name="propertyFactory">Optional <seealso cref="IPropertyFactory"/>.</param>
        /// <param name="propertyValueFactory"><see cref="IPropertyValue"/> factory. Default: <see cref="CachedPropertyValueFactory"/>.</param>
        /// <param name="validationFactory">Validation factory to create validation rules.</param>
        /// <param name="validateOnParse">Properties should be validated on parse.</param>
        /// <param name="setSchemaForObjects">Schema should be set for every <see cref="IPropertyContainer"/> object.</param>
        public XmlParserSettings(
            Func<XElement, string>? getElementName = null,
            IReadOnlyCollection<IParserRule>? parserRules = null,
            IStringProvider? stringProvider = null,
            IEqualityComparer<IProperty>? propertyComparer = null,
            IPropertyFactory? propertyFactory = null,
            IPropertyValueFactory? propertyValueFactory = null,
            IValidationProvider? validationFactory = null,
            bool validateOnParse = false,
            bool setSchemaForObjects = true)
        {
            GetElementName = getElementName ?? XmlParser.GetElementNameDefault;
            ParserRules = parserRules ?? XmlParser.CreateDefaultXmlParsersRules().ToArray();
            StringProvider = stringProvider ?? new DefaultStringProvider();
            PropertyComparer = propertyComparer ?? Metadata.PropertyComparer.ByReferenceComparer;
            PropertyFactory = propertyFactory ?? Metadata.PropertyFactory.Default.Cached();
            PropertyValueFactory = propertyValueFactory ?? new PropertyValueFactory().Cached(PropertyComparer);

            ValidationProvider = validationFactory ?? Validation.Rules.ValidationProvider.Instance;
            ValidateOnParse = validateOnParse;
            SetSchemaForObjects = setSchemaForObjects;
        }
    }
}
