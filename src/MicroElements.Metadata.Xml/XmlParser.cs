using System;
using System.Xml;
using System.Xml.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    public class XmlParser
    {
        public class XmlParseInfo
        {
            public bool IsFromSchema { get; set; } = true;
        }

        public static IPropertyContainer ParseXmlDocument(XDocument document, ISchema schema)
        {
            var container = new MutablePropertyContainer();

            XElement? rootElement = document.Root;
            if (rootElement != null && rootElement.HasElements)
            {
                foreach (XElement propertyElement in rootElement.Elements())
                {
                    IProperty? property = schema.GetProperty(propertyElement.Name.LocalName);

                    if (propertyElement.HasElements)
                    {
                        ISchema propertySchema = property?.GetOrAddSchema() ?? new Schema.Schema();

                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertySchema);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            internalObject.SetSchema(propertySchema);
                            if (property == null)
                            {
                                property = schema.AddProperty(new Property<IPropertyContainer>(propertyElement.Name.LocalName)
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
                            property = schema.AddProperty(new Property<string>(propertyElement.Name.LocalName)
                                .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false));
                        }

                        if (property.Type == typeof(string))
                            container.Add(PropertyValue.Create(property, propertyElement.Value));
                    }
                }
            }

            return container;
        }

        public static IPropertyContainer? ParseXmlElement(XElement objectElement, ISchema objectSchema)
        {
            if (objectElement.HasElements)
            {
                var container = new MutablePropertyContainer();

                foreach (XElement propertyElement in objectElement.Elements())
                {
                    IProperty? property = objectSchema.GetProperty(propertyElement.Name.LocalName);

                    if (property != null)
                    {
                        // Property has schema in metadata.
                        if (property.Type.IsAssignableTo<IPropertyContainer>() && property.GetSchema() is { } hasSchema)
                        {
                            ISchema propertySchema = hasSchema.Schema.ToSchema();
                            var compositeValue = ParseXmlElement(propertyElement, propertySchema);
                            container.Add(PropertyValue.Create(property, compositeValue));

                            continue;
                        }
                    }

                    if (propertyElement.HasElements)
                    {
                        ISchema propertyInternalSchema = property?.GetOrAddSchema() ?? new Schema.Schema();
                        IPropertyContainer? internalObject = ParseXmlElement(propertyElement, propertyInternalSchema);
                        if (internalObject != null && internalObject.Count > 0)
                        {
                            internalObject.SetSchema(propertyInternalSchema);
                            if (property == null)
                            {
                                property = objectSchema.AddProperty(new Property<IPropertyContainer>(propertyElement.Name.LocalName)
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
                            property = objectSchema.AddProperty(new Property<string>(propertyElement.Name.LocalName)
                                .ConfigureMetadata<IProperty, XmlParseInfo>(info => info.IsFromSchema = false));
                        }
                        container.Add(PropertyValue.Create(property, propertyElement.Value));
                    }
                }

                return container;
            }

            return null;
        }

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
                        if (property.Type.IsAssignableTo<IPropertyContainer>() && property.GetSchema() is { } hasSchema)
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
