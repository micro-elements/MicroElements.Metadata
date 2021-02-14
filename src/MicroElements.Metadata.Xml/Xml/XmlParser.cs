// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Parsing;
using MicroElements.Metadata.Schema;
using MicroElements.Validation;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Parses xml to <see cref="IPropertyContainer"/>.
    /// </summary>
    public static partial class XmlParser
    {
        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="stream">Stream with xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="context">Optional parse context.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this Stream stream,
            ISchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null,
            IMutablePropertyContainer? container = null)
        {
            XDocument xDocument = XDocument.Load(stream, LoadOptions.SetLineInfo);
            return ParseXmlDocument(xDocument, schema, settings, context, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="text">A string that contains Xml.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="context">Optional parse context.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this string text,
            ISchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null,
            IMutablePropertyContainer? container = null)
        {
            XDocument xDocument = XDocument.Parse(text, LoadOptions.SetLineInfo);
            return ParseXmlDocument(xDocument, schema, settings, context, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="context">Optional parse context.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this XDocument document,
            ISchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null,
            IMutablePropertyContainer? container = null)
        {
            return ParseXmlDocument(document, schema, settings, context, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="context">Optional parse context.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlDocument(
            XDocument document,
            ISchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null,
            IMutablePropertyContainer? container = null)
        {
            document.AssertArgumentNotNull(nameof(document));

            container ??= new MutablePropertyContainer();

            XElement? rootElement = document.Root;
            if (rootElement != null && rootElement.HasElements)
            {
                settings ??= new XmlParserSettings();
                context ??= new XmlParserContext(settings, schema);
                schema ??= context.Schema;

                container.SetSchema(schema);
                container.SetMetadata(context);

                ParseXmlElement(rootElement, schema, settings, context, container);
            }

            return container;
        }

        private static IPropertyContainer? ParseXmlElement(
            XElement objectElement,
            ISchema objectSchema,
            IXmlParserSettings settings,
            IXmlParserContext context,
            IMutablePropertyContainer? container = null)
        {
            if (objectElement.HasElements)
            {
                container ??= new MutablePropertyContainer();

                foreach (XElement propertyElement in objectElement.Elements())
                {
                    string propertyName = settings.StringProvider.GetString(settings.GetElementName(propertyElement));
                    IProperty? property = objectSchema.GetProperty(propertyName);

                    if (propertyElement.HasElements)
                    {
                        ISchema propertyInternalSchema = context.GetOrAddSchema(property);

                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertyInternalSchema, settings, context);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            internalObject.SetSchema(propertyInternalSchema);

                            if (property == null)
                            {
                                property = objectSchema
                                    .AddProperty(new Property<IPropertyContainer>(propertyName)
                                        .SetIsNotFromSchema()
                                        .SetSchema(propertyInternalSchema));
                            }

                            container.Add(settings.PropertyValueFactory.CreateUntyped(property, internalObject));

                            // Validate property.
                            ValidateProperty(context, container, property, propertyElement);
                        }
                    }
                    else
                    {
                        if (property != null && property.Type == typeof(IPropertyContainer))
                        {
                            // Composite object, no value.
                            bool isNullAllowed = property.GetOrEvaluateAllowNull().IsNullAllowed;
                            if (!isNullAllowed)
                            {
                                context.Messages.AddError(
                                    $"Property '{property.Name}' can not be null but xml element has no value.{GetXmlLineInfo(propertyElement)}");
                            }

                            continue;
                        }

                        if (property == null)
                        {
                            property = objectSchema
                                .AddProperty(new Property<string>(propertyName)
                                    .SetIsNotFromSchema());
                        }

                        IValueParser valueParser = context.GetParserCached(property);
                        if (valueParser != EmptyParser.Instance)
                        {
                            string elementValue = propertyElement.Value;

                            // Parse value.
                            IParseResult parseResult = valueParser.ParseUntyped(elementValue);

                            if (parseResult.IsSuccess)
                            {
                                // Add property to container.
                                object? parsedValue = parseResult.ValueUntyped;
                                container.Add(settings.PropertyValueFactory.CreateUntyped(property, parsedValue));

                                // Validate property.
                                ValidateProperty(context, container, property, propertyElement);
                            }
                            else
                            {
                                string? parseResultErrorMessage = parseResult.Error?.FormattedMessage;
                                string parseResultError = parseResultErrorMessage != null ? $" Error: '{parseResultErrorMessage}'." : string.Empty;
                                string errorMessage = $"Property '{property.Name}' failed to parse from string '{elementValue}'.{parseResultError}{GetXmlLineInfo(propertyElement)}";
                                context.Messages.AddError(errorMessage);
                            }
                        }
                        else
                        {
                            string errorMessage = $"Property '{property.Name}' can not be parsed because no parser found for type {property.Type}.{GetXmlLineInfo(propertyElement)}";
                            context.Messages.AddError(errorMessage);
                        }
                    }
                }

                return container;
            }

            return null;
        }

        private static void ValidateProperty(
            IXmlParserContext context,
            IMutablePropertyContainer container,
            IProperty property,
            XElement propertyElement)
        {
            if (context.ParserSettings.ValidateOnParse)
            {
                var validationRules = context.GetValidatorsCached(property);
                if (validationRules.Rules.Count > 0)
                {
                    container
                        .Validate(validationRules.Rules)
                        .Select(message => message.WithText(message.OriginalMessage + GetXmlLineInfo(propertyElement)))
                        .Iterate(message => context.Messages.Add(message));
                }
            }
        }

        private static string GetXmlLineInfo(this XElement element)
        {
            if (element is IXmlLineInfo xmlLineInfo)
            {
                if (xmlLineInfo.HasLineInfo())
                    return $" LineNumber: {xmlLineInfo.LineNumber}, LinePosition: {xmlLineInfo.LinePosition}.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Alternative version with XmlReader.
        /// </summary>
        public static object? ReadXmlElement(
            XmlReader xmlReader,
            ISchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null)
        {
            int rootDepth = xmlReader.Depth;
            string? elementName = null;

            MutablePropertyContainer? container = null;
            IProperty? property = null;

            settings ??= new XmlParserSettings();
            context ??= new XmlParserContext(settings, schema);
            schema ??= context.Schema;

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (container == null)
                    {
                        container = new MutablePropertyContainer();
                        container.SetSchema(schema);
                        container.SetMetadata(context);
                    }

                    elementName = xmlReader.Name;
                    property = schema.GetProperty(elementName);

                    if (property != null)
                    {
                        ISchema propertySchema = context.GetOrAddSchema(property);
                        var compositeValue = ReadXmlElement(xmlReader, propertySchema, settings, context);
                        container.Add(settings.PropertyValueFactory.CreateUntyped(property, compositeValue));
                    }
                    else if (xmlReader.Depth > rootDepth)
                    {
                        ISchema propertySchema = context.GetOrAddSchema(property);
                        var compositeValue = ReadXmlElement(xmlReader, propertySchema, settings, context);

                        if (compositeValue is IPropertyContainer internalObject)
                        {
                            property = Property
                                .Create(typeof(IPropertyContainer), elementName)
                                .SetSchema(propertySchema);
                            schema.AddProperty(property);
                            container.Add(settings.PropertyValueFactory.CreateUntyped(property, internalObject));
                        }
                        else
                        {
                            property = Property.Create(typeof(string), elementName);
                            schema.AddProperty(property);
                            container.Add(settings.PropertyValueFactory.CreateUntyped(property, compositeValue));
                        }
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    if (elementName != null && container != null)
                    {
                        property ??= schema.AddProperty(Property.Create(xmlReader.ValueType, elementName));

                        object value = xmlReader.ReadContentAs(property.Type, null);
                        container.Add(settings.PropertyValueFactory.CreateUntyped(property, value));
                    }
                    else
                    {
                        object value = xmlReader.ReadContentAs(typeof(string), null);
                        return value;
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            return container;
        }
    }
}
