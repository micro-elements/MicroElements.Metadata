// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
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
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseToContainer(
            this XDocument document,
            ISchema? schema = null,
            XmlParserSettings? settings = null)
        {
            return ParseXmlDocument(document, schema, settings);
        }

        /// <summary>
        /// Parses xml document to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="document">Xml document.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="settings">Optional parse settings.</param>
        /// <returns><see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer ParseXmlDocument(
            XDocument document,
            ISchema? schema = null,
            XmlParserSettings? settings = null)
        {
            document.AssertArgumentNotNull(nameof(document));

            var container = new MutablePropertyContainer();

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
                        if (property == null)
                        {
                            property = objectSchema
                                .AddProperty(new Property<string>(propertyName)
                                    .SetIsNotFromSchema());
                        }

                        if (property.Type == typeof(string))
                        {
                            container.Add(PropertyValue.Create(property, propertyElement.Value));
                        }
                        else
                        {
                            IParserRule? parserRule = settings.ParserRules.GetParser(property);
                            if (parserRule != null)
                            {
                                Option<object> parseResult = parserRule.Parser.ParseUntyped(propertyElement.Value);

                                if (parseResult.IsSome)
                                {
                                    object? parsedValue = parseResult!.GetValueOrDefault();
                                    container.Add(PropertyValue.Create(property, parsedValue));
                                }
                            }
                            else
                            {
                                int can_not_parse = 1;
                            }

                            // HasElements==false, Type is not string
                        }
                    }
                }

                return container;
            }

            return null;
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
