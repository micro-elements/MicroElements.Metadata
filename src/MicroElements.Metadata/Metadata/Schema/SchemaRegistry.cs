// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.Schema
{
    // TODO: Schema functional will be extended next releases!

    internal interface ISchemaRegistry
    {
        public void Add(string schemaId, ISchema schema);

        public void Add(IProperty property, ISchema schema);

        public ISchema Get(string schemaId);

        public ISchema Get(IProperty property);

        public IReadOnlyCollection<ISchema> Schemas { get; }

        public ConcurrentDictionary<IProperty, ISchema> SchemaCache { get; }
    }

    public static partial class SchemaExtensions
    {
        public static ISchema ToSchema(this IPropertySet propertySet)
        {
            if (propertySet is ISchema schema)
            {
                return schema;
            }

            return new MutableSchema(propertySet);
        }
    }

    public class MutableSchema : ISchema
    {
        private readonly List<IProperty> _properties = new List<IProperty>();
        private readonly Dictionary<string, IProperty> _dictionary;

        public MutableSchema(IEnumerable<IProperty>? properties = null)
        {
            if (properties != null)
            {
                _properties.AddRange(properties);
            }

            _dictionary = _properties.ToDictionary(property => property.Name, property => property);
        }

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties()
        {
            return _properties;
        }

        /// <inheritdoc />
        public IProperty AddProperty(IProperty property)
        {
            _properties.Add(property);
            _dictionary.TryAdd(property.Name, property);

            return property;
        }

        /// <inheritdoc />
        public IProperty? GetProperty(string name)
        {
            _dictionary.TryGetValue(name, out var value);
            return value;
        }
    }
}
