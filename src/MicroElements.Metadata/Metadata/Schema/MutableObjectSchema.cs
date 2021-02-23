﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.Schema
{
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
        public ISchemaCombinator? Combinator { get; }

        public MutableObjectSchema(IEnumerable<IProperty>? properties = null)
        {
            if (properties != null)
            {
                _properties.AddRange(properties);
            }

            _dictionary = _properties.ToDictionary(property => property.Name, property => property);
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
