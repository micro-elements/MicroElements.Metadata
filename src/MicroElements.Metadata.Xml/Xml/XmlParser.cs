// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;
using System.Xml.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// Parses xml to <see cref="IPropertyContainer"/>.
    /// </summary>
    public static class XmlParser
    {
        public class XmlParseInfo
        {
            public bool IsFromSchema { get; set; } = true;
        }

        public class Settings
        {
            public Func<XElement, string>? GetElementName { get; set; }
        }

        /// <summary>
        /// Gets full element name from root parent to current element.
        /// Example: "A.B.C".
        /// </summary>
        /// <param name="element">Source element.</param>
        /// <param name="delimeter">Optional delimeter.</param>
        /// <returns>Full element name.</returns>
        public static string GetFullName(this XElement element, string delimeter = ".")
        {
            string name = element.Name.LocalName;

            XElement? parent = element.Parent;
            while (parent != null)
            {
                name = parent.Name.LocalName + delimeter + name;
                parent = parent.Parent;
            }

            return name;
        }

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
            Settings? settings = null)
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
            Settings? settings = null)
        {
            document.AssertArgumentNotNull(nameof(document));

            var container = new MutablePropertyContainer();

            XElement? rootElement = document.Root;
            if (rootElement != null && rootElement.HasElements)
            {
                schema ??= new Schema.Schema();
                settings ??= new Settings();

                foreach (XElement propertyElement in rootElement.Elements())
                {
                    string propertyName = settings.GetElementName?.Invoke(propertyElement) ?? propertyElement.Name.LocalName;
                    IProperty? property = schema.GetProperty(propertyName);

                    if (propertyElement.HasElements)
                    {
                        ISchema propertySchema = property?.GetOrAddSchema() ?? new Schema.Schema();

                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertySchema, settings);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            internalObject.SetSchema(propertySchema);
                            if (property == null)
                            {
                                property = schema.AddProperty(new Property<IPropertyContainer>(propertyName)
                                    .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false))
                                    .SetSchema(propertySchema);
                            }
                            container.Add(PropertyValue.Create(property, internalObject));
                        }
                    }
                    else
                    {
                        if (property == null)
                        {
                            property = schema.AddProperty(new Property<string>(propertyName)
                                .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false));
                        }

                        if (property.Type == typeof(string))
                            container.Add(PropertyValue.Create(property, propertyElement.Value));
                    }
                }
            }

            return container;
        }

        public static IPropertyContainer? ParseXmlElement(XElement objectElement, ISchema objectSchema, Settings settings)
        {
            if (objectElement.HasElements)
            {
                var container = new MutablePropertyContainer();

                foreach (XElement propertyElement in objectElement.Elements())
                {
                    string propertyName = settings.GetElementName?.Invoke(propertyElement) ?? propertyElement.Name.LocalName;
                    IProperty? property = objectSchema.GetProperty(propertyName);

                    if (property != null)
                    {
                        // Property has schema in metadata.
                        if (property.Type.IsAssignableTo<IPropertyContainer>() && property.GetHasSchema() is { } hasSchema)
                        {
                            ISchema propertySchema = hasSchema.Schema.ToSchema();
                            var compositeValue = ParseXmlElement(propertyElement, propertySchema, settings);
                            container.Add(PropertyValue.Create(property, compositeValue));

                            continue;
                        }
                    }

                    if (propertyElement.HasElements)
                    {
                        ISchema propertyInternalSchema = property?.GetOrAddSchema() ?? new Schema.Schema();
                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertyInternalSchema, settings);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            internalObject.SetSchema(propertyInternalSchema);
                            if (property == null)
                            {
                                property = objectSchema.AddProperty(new Property<IPropertyContainer>(propertyName)
                                    .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false))
                                    .SetSchema(propertyInternalSchema);
                            }
                            container.Add(PropertyValue.Create(property, internalObject));
                        }
                    }
                    else
                    {
                        if (property == null)
                        {
                            property = objectSchema.AddProperty(new Property<string>(propertyName)
                                .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false));
                        }
                        container.Add(PropertyValue.Create(property, propertyElement.Value));
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
