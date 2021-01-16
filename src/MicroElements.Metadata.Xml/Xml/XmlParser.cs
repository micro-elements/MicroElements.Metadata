// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

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
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this Stream stream,
            ISchema? schema = null,
            XmlParserSettings? settings = null,
            IMutablePropertyContainer? container = null)
        {
            XDocument xDocument = XDocument.Load(stream, LoadOptions.SetLineInfo);
            return ParseXmlDocument(xDocument, schema, settings, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="text">A string that contains Xml.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this string text,
            ISchema? schema = null,
            XmlParserSettings? settings = null,
            IMutablePropertyContainer? container = null)
        {
            XDocument xDocument = XDocument.Parse(text, LoadOptions.SetLineInfo);
            return ParseXmlDocument(xDocument, schema, settings, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlToContainer(
            this XDocument document,
            ISchema? schema = null,
            XmlParserSettings? settings = null,
            IMutablePropertyContainer? container = null)
        {
            return ParseXmlDocument(document, schema, settings, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="stream">Stream with xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlDocument(
            this Stream stream,
            ISchema? schema = null,
            XmlParserSettings? settings = null,
            IMutablePropertyContainer? container = null)
        {
            stream.AssertArgumentNotNull(nameof(stream));

            XDocument xDocument = XDocument.Load(stream, LoadOptions.SetLineInfo);
            return ParseXmlDocument(xDocument, schema, settings, container);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <param name="container">Target property container.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlDocument(
            XDocument document,
            ISchema? schema = null,
            XmlParserSettings? settings = null,
            IMutablePropertyContainer? container = null)
        {
            document.AssertArgumentNotNull(nameof(document));

            container ??= new MutablePropertyContainer();

            XElement? rootElement = document.Root;
            if (rootElement != null && rootElement.HasElements)
            {
                schema ??= new Schema.Schema();
                settings ??= new XmlParserSettings();
                IXmlParserSettings parserSettings = new ReadOnlyXmlParserSettings(settings);

                container.SetSchema(schema);

                ParseXmlElement(rootElement, schema, parserSettings, container);
            }

            return container;
        }

        private static IPropertyContainer? ParseXmlElement(
            XElement objectElement,
            ISchema objectSchema,
            IXmlParserSettings settings,
            IMutablePropertyContainer? container = null)
        {
            if (objectElement.HasElements)
            {
                container ??= new MutablePropertyContainer();

                foreach (XElement propertyElement in objectElement.Elements())
                {
                    string propertyName = settings.GetElementName(propertyElement);
                    IProperty? property = objectSchema.GetProperty(propertyName);

                    if (propertyElement.HasElements)
                    {
                        ISchema propertyInternalSchema = property?.GetOrAddSchema() ?? new Schema.Schema();
                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertyInternalSchema, settings);
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

                            container.Add(PropertyValue.Create(property, internalObject));
                        }
                    }
                    else
                    {
                        if (property != null && property.Type == typeof(IPropertyContainer))
                        {
                            // Composite object, no value.
                            // TODO: Can it be null?
                        }

                        if (property == null)
                        {
                            property = objectSchema
                                .AddProperty(new Property<string>(propertyName)
                                    .SetIsNotFromSchema());
                        }

                        // TODO: cache: property->parser
                        IParserRule? parserRule = settings.ParserRules.GetParser(property);
                        if (parserRule != null)
                        {
                            string elementValue = propertyElement.Value;

                            // WARN: boxing
                            Option<object> parseResult = parserRule.Parser.ParseUntyped(elementValue);

                            if (parseResult.IsSome)
                            {
                                object? parsedValue = parseResult!.GetValueOrDefault();
                                container.Add(PropertyValue.Create(property, parsedValue));
                            }
                            else
                            {
                                settings.Messages.AddError(
                                    $"Property '{property.Name}' was not parsed from string '{elementValue}'.{GetXmlLineInfo(propertyElement)}");
                            }
                        }
                        else
                        {
                            settings.Messages.AddError(
                                $"Property '{property.Name}' can not be parsed because no parser found for type {property.Type}.{GetXmlLineInfo(propertyElement)}");
                        }
                    }
                }

                return container;
            }

            return null;
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
        public static object? ReadXmlElement(XmlReader xmlReader, ISchema schema)
        {
            int rootDepth = xmlReader.Depth;
            string? elementName = null;

            MutablePropertyContainer? container = null;
            IProperty? property = null;

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (container == null)
                    {
                        container = new MutablePropertyContainer();
                        container.SetSchema(schema);
                    }
                    elementName = xmlReader.Name;

                    property = schema.GetProperty(elementName);

                    if (property != null)
                    {
                        // Property has schema in metadata.
                        if (property.Type.IsAssignableTo<IPropertyContainer>() && property.GetHasSchema() is { } hasSchema)
                        {
                            ISchema propertySchema = hasSchema.Schema.ToSchema();
                            var compositeValue = ReadXmlElement(xmlReader, propertySchema);
                            container.Add(PropertyValue.Create(property, compositeValue));

                            continue;
                        }

                        // Property type is IPropertySet
                        if (property.Type.IsAssignableTo(typeof(IPropertySet)))
                        {
                            ISchema propertySchema = ((IPropertySet)Activator.CreateInstance(property.Type)!).ToSchema();
                            var compositeValue = ReadXmlElement(xmlReader, propertySchema);
                            container.Add(PropertyValue.Create(Property.Create(typeof(IPropertyContainer), property.Name), compositeValue));

                            continue;
                        }

                        if (xmlReader.Depth > rootDepth)
                        {
                            var compositeValue = ReadXmlElement(xmlReader, schema);
                            container.Add(PropertyValue.Create(property, compositeValue));
                        }
                    }
                    else if (xmlReader.Depth > rootDepth)
                    {
                        ISchema propertySchema = property?.GetOrAddSchema() ?? new Schema.Schema();

                        var compositeValue = ReadXmlElement(xmlReader, propertySchema);
                        if(compositeValue is IPropertyContainer internalObject)
                        {
                            property = Property
                                .Create(typeof(IPropertyContainer), elementName)
                                .SetSchema(propertySchema);
                            schema.AddProperty(property);
                            container.Add(PropertyValue.Create(property, internalObject));
                        }
                        else
                        {
                            property = Property.Create(typeof(string), elementName);
                            schema.AddProperty(property);
                            container.Add(PropertyValue.Create(property, compositeValue));
                        }
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    if (elementName != null && container != null)
                    {
                        property ??= schema.AddProperty(Property.Create(xmlReader.ValueType, elementName));

                        object value = xmlReader.ReadContentAs(property.Type, null);
                        container.Add(PropertyValue.Create(property, value));
                    }
                    else
                    {
                        object value = xmlReader.ReadContentAs(typeof(string), null);
                        return value;
                    }
                }
                else if(xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            return container;
        }
    }
}
