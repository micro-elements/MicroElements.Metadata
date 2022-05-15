// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
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
            IObjectSchema? schema = null,
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
            IObjectSchema? schema = null,
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
            IObjectSchema? schema = null,
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

                ParseXmlElement(rootElement, (IObjectSchema)schema, settings, context, container);
            }

            return container;
        }

        private static IPropertyContainer? ParseXmlElement(
            XElement objectElement,
            IObjectSchema objectSchema,
            IXmlParserSettings settings,
            IXmlParserContext context,
            IMutablePropertyContainer? container = null)
        {
            if (objectElement.HasElements)
            {
                container ??= new MutablePropertyContainer();

                foreach (XElement propertyElement in objectElement.Elements())
                {
                    string elementName = settings.GetElementName(propertyElement);
                    string propertyName = settings.StringProvider.GetString(elementName);
                    IProperty? property = objectSchema.GetProperty(propertyName);

                    if (propertyElement.HasElements)
                    {
                        IObjectSchema propertyInternalSchema = context.GetOrCreateNewSchemaCached(property).ToObjectSchema();

                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertyInternalSchema, settings, context);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            if (settings.SetSchemaForObjects)
                                internalObject.SetSchema(propertyInternalSchema);

                            if (property == null)
                            {
                                property = new Property<IPropertyContainer>(propertyName)
                                    .SetIsNotFromSchema()
                                    .SetSchema(propertyInternalSchema);

                                if (objectSchema is IMutableObjectSchema mutableObjectSchema)
                                {
                                    property = mutableObjectSchema.AddProperty(property);
                                }
                            }

                            IPropertyValue propertyValue = settings.PropertyValueFactory.CreateUntyped(property, internalObject);
                            container.Add(propertyValue);

                            // Validate property.
                            if (settings.ValidateOnParse)
                                ValidateProperty(context, container, property, propertyElement);
                        }
                    }
                    else
                    {
                        if (property != null && property.Type == typeof(IPropertyContainer))
                        {
                            // Composite object, no value.
                            bool isNullAllowed = property.GetOrEvaluateNullability().IsNullAllowed;
                            if (!isNullAllowed)
                            {
                                Message message = ValueMessageBuilder
                                    .Error("Property '{PropertyName}' can not be null but xml element has no value.")
                                    .AddProperty("PropertyName", property.Name)
                                    .AppendXmlLineInfo(propertyElement)
                                    .Build();

                                context.Messages.Add(message);
                            }

                            continue;
                        }

                        if (property == null)
                        {
                            property = new Property<string>(propertyName)
                                .SetIsNotFromSchema();

                            if (objectSchema is IMutableObjectSchema mutableObjectSchema)
                            {
                                property = mutableObjectSchema.AddProperty(property);
                            }
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
                                IPropertyValue propertyValue = settings.PropertyValueFactory.CreateUntyped(property, parsedValue);
                                container.Add(propertyValue);

                                // Validate property.
                                if (settings.ValidateOnParse)
                                    ValidateProperty(context, container, property, propertyElement);
                            }
                            else
                            {
                                string? parseResultErrorMessage = parseResult.Error?.FormattedMessage;

                                var message =
                                    ValueMessageBuilder
                                        .Error("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.")
                                        .AddProperty("PropertyName", property.Name)
                                        .AddProperty("PropertyValue", elementValue)
                                        .If(parseResultErrorMessage != null)
                                          .AppendToOriginalMessage(" Error: '{ParseResultError}'.")
                                          .AddProperty("ParseResultError", parseResultErrorMessage)
                                        .EndIf()
                                        .AppendXmlLineInfo(propertyElement)
                                        .Build();

                                context.Messages.Add(message);
                            }
                        }
                        else
                        {
                            Message message = ValueMessageBuilder
                                .Error("Property '{PropertyName}' can not be parsed because no parser found for type '{PropertyType}'.}")
                                .AddProperty("PropertyName", property.Name)
                                .AddProperty("PropertyType", property.Type)
                                .AppendXmlLineInfo(propertyElement)
                                .Build();

                            context.Messages.Add(message);
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
            var validationRules = context.GetValidatorsCached(property);
            if (validationRules.Rules.Count > 0)
            {
                IEnumerable<Message> messages = container.Validate(validationRules.Rules);
                foreach (Message message in messages)
                {
                    context.Messages.Add(message.AppendXmlLineInfo(propertyElement));
                }
            }
        }

        public static ValueMessageBuilder AppendXmlLineInfo(this ValueMessageBuilder messageBuilder, IXmlLineInfo xmlLineInfo)
        {
            if (xmlLineInfo.HasLineInfo())
            {
                return messageBuilder
                    .AppendToOriginalMessage(" LineNumber: {LineNumber}, LinePosition: {LinePosition}.")
                    .AddProperty("LineNumber", xmlLineInfo.LineNumber)
                    .AddProperty("LinePosition", xmlLineInfo.LinePosition);
            }

            return messageBuilder;
        }

        private static Message AppendXmlLineInfo(this Message message, XElement element)
        {
            if (element is IXmlLineInfo xmlLineInfo)
            {
                if (xmlLineInfo.HasLineInfo())
                {
                    return ValueMessageBuilder
                        .FromMessage(message)
                        .AppendXmlLineInfo(element)
                        .Build();
                }
            }

            return message;
        }

        /// <summary>
        /// Alternative version with XmlReader.
        /// </summary>
        public static object? ReadXmlElement(
            XmlReader xmlReader,
            IObjectSchema? schema = null,
            IXmlParserSettings? settings = null,
            IXmlParserContext? context = null)
        {
            int rootDepth = xmlReader.Depth;
            string? elementName = null;

            MutablePropertyContainer? container = null;
            IProperty? property = null;

            settings ??= new XmlParserSettings();
            context ??= new XmlParserContext(settings, schema);
            schema ??= (IObjectSchema)context.Schema;
            IMutableObjectSchema mutableObjectSchema = schema.ToMutableObjectSchema();

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
                        var propertySchema = context.GetOrCreateNewSchemaCached(property).ToObjectSchema();
                        var compositeValue = ReadXmlElement(xmlReader, propertySchema, settings, context);
                        container.Add(settings.PropertyValueFactory.CreateUntyped(property, compositeValue));
                    }
                    else if (xmlReader.Depth > rootDepth)
                    {
                        var propertySchema = context.GetOrCreateNewSchemaCached(property).ToObjectSchema();
                        var compositeValue = ReadXmlElement(xmlReader, propertySchema, settings, context);

                        if (compositeValue is IPropertyContainer internalObject)
                        {
                            property = settings.PropertyFactory
                                .Create(typeof(IPropertyContainer), elementName)
                                .SetSchema(propertySchema);
                            mutableObjectSchema.AddProperty(property);
                            container.Add(settings.PropertyValueFactory.CreateUntyped(property, internalObject));
                        }
                        else
                        {
                            property = settings.PropertyFactory.Create(typeof(string), elementName);
                            mutableObjectSchema.AddProperty(property);
                            container.Add(settings.PropertyValueFactory.CreateUntyped(property, compositeValue));
                        }
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    if (elementName != null && container != null)
                    {
                        property ??= mutableObjectSchema.AddProperty(settings.PropertyFactory.Create(xmlReader.ValueType, elementName));

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
