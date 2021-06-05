// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

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
        void AddSchema(string schemaId, ISchema schema);

        ISchema? GetSchema(string schemaId);
    }

    public class SchemaRepository : ISchemaRepository
    {
        private Dictionary<string, ISchema> _schemas = new Dictionary<string, ISchema>();

        /// <inheritdoc />
        public void AddSchema(string schemaId, ISchema schema)
        {
            _schemas[schemaId] = schema;
        }

        /// <inheritdoc />
        public ISchema? GetSchema(string schemaId)
        {
            _schemas.TryGetValue(schemaId, out var result);
            return result;
        }
    }
}
