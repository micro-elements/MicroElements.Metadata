// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.Schema
{
    internal interface ISchemaRegistry
    {
        public void Add(string id, object schema);

        public object Get(string id);

        public IReadOnlyCollection<object> Schemas { get; }
    }

    public interface ISchema : IPropertySet
    {
        IProperty AddProperty(IProperty property);

        IProperty? GetProperty(string name);
    }

    public static partial class SchemaExtensions
    {
        public static ISchema ToSchema(this IPropertySet propertySet) => new Schema(propertySet);
    }

    public class Schema : ISchema
    {
        private readonly List<IProperty> _properties = new List<IProperty>();
        private readonly Dictionary<string, IProperty> _dictionary;

        public Schema(IEnumerable<IProperty>? properties = null)
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
