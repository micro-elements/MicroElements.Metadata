// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;

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
        public static IMutableObjectSchema ToSchema(this IPropertySet propertySet)
        {
            if (propertySet is IMutableObjectSchema schema)
            {
                return schema;
            }

            return new MutableObjectSchema(name: propertySet.GetType().Name, properties: propertySet);
        }
    }
}
