// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Mutable object schema.
    /// </summary>
    public class MutableObjectSchema : IMutableObjectSchema
    {
        private readonly List<IProperty> _properties = new List<IProperty>();
        private readonly Dictionary<string, IProperty> _dictionary;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IProperty> Properties => _properties;

        /// <inheritdoc />
        public ISchemaCombinator? Combinator => this.GetSchemaCombinator(CombinatorType.AllOf);

        public MutableObjectSchema(
            string? name = null,
            Type? type = null,
            string? description = null,
            IEnumerable<IProperty>? properties = null,
            ISchemaCombinator? combinator = null)
        {
            Name = name;
            Type = type ?? typeof(IPropertyContainer);
            Description = description;

            if (combinator != null)
                this.SetSchemaCombinator(combinator);

            if (properties != null)
            {
                _properties.AddRange(properties);
            }

            _dictionary = _properties.ToDictionary(property => property.Name, property => property);
        }

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties() => _properties;

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}. Mutable, Count: {_properties.Count}}}";
        }
    }

    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        public static IMutableObjectSchema ToSchema(this IPropertySet propertySet, string? schemaName = null)
        {
            if (propertySet is IMutableObjectSchema schema)
            {
                return schema;
            }

            string? name = schemaName;

            if (name == null && propertySet is IKnownPropertySet knownPropertySet)
            {
                name = knownPropertySet.SchemaType.Name;
            }

            if (name == null)
            {
                name = propertySet.GetType().Name;
            }

            return new MutableObjectSchema(name: name, properties: propertySet.GetProperties());
        }
    }
}
