﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides schema.
    /// </summary>
    public interface ISchemaProvider
    {
        /// <summary>
        /// Gets schema instance.
        /// </summary>
        /// <returns>Schema instance.</returns>
        ISchema GetSchema();
    }

    /// <summary>
    /// Provides object schema.
    /// </summary>
    public interface IObjectSchemaProvider : ISchemaProvider
    {
        /// <inheritdoc />
        ISchema ISchemaProvider.GetSchema() => GetObjectSchema();

        /// <summary>
        /// Gets schema instance.
        /// </summary>
        /// <returns>Schema instance.</returns>
        IObjectSchema GetObjectSchema();
    }

    public interface ISchemaRepository
    {
        IEnumerable<KeyValuePair<string, ISchema>> GetSchemas();

        string AddSchema(string schemaId, ISchema schema);

        ISchema? GetSchema(string schemaId);
    }

    public interface IMetadataSchemaProvider : IMetadataProvider
    {
        public ISchemaRepository Schemas
        {
            get
            {
                ISchemaRepository? schemaRepository = this.GetMetadata<ISchemaRepository>();
                if (schemaRepository == null)
                {
                    schemaRepository = new SchemaRepository();
                    this.SetMetadata(schemaRepository);
                }

                return schemaRepository;
            }

            set
            {
                this.SetMetadata(value);
            }
        }
    }

    public class SchemaRepository : ISchemaRepository
    {
        private Dictionary<string, ISchema> _schemas = new Dictionary<string, ISchema>();
        private ConcurrentDictionary<string, ISchema> _schemasByDigest = new ConcurrentDictionary<string, ISchema>();

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, ISchema>> GetSchemas()
        {
            return _schemas;
        }

        /// <inheritdoc />
        public string AddSchema(string schemaId, ISchema schema)
        {
            if (schema is IObjectSchema objectSchema)
            {
                string schemaDigest = SchemaEqualityComparer.Instance.GetSchemaDigest(objectSchema.Properties);
                schema = _schemasByDigest.GetOrAdd(schemaDigest, s => schema);
                _schemas[schema.Name] = objectSchema;
                return schema.Name;
            }
            else
            {
                _schemas[schemaId] = schema;
                return schemaId;
            }
        }

        /// <inheritdoc />
        public ISchema? GetSchema(string schemaId)
        {
            _schemas.TryGetValue(schemaId, out var result);
            return result;
        }
    }

    public class SchemaEqualityComparer
    {
        public static SchemaEqualityComparer Instance = new SchemaEqualityComparer();

        public string GetSchemaDigest(IReadOnlyCollection<IProperty> properties)
        {
            //MetadataSchema.GenerateCompactSchema(properties)
            string digest = properties
                .OrderBy(property => property.Name)
                .Aggregate(
                    new StringBuilder(),
                    (builder, property) => builder.AppendFormat("{0}@{1};", property.Name, property.Type))
                .ToString();
            return digest;
        }
    }
}
