// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static partial class SchemaBuilder
    {
        /// <summary>
        /// Creates schema copy with provided description.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="schemaDescription">Description.</param>
        /// <returns>New schema instance with provided description.</returns>
        public static TSchema WithDescription<TSchema>(this TSchema source, ISchemaDescription schemaDescription)
            where TSchema : ISchemaBuilder<ISchemaDescription>, ISchema
        {
            return source.WithComponent(schemaDescription);
        }

        /// <summary>
        /// Creates schema copy with provided description.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="description">Description.</param>
        /// <returns>New schema instance with provided description.</returns>
        public static TSchema WithDescription<TSchema>(this TSchema source, string description)
            where TSchema : ISchemaBuilder<ISchemaDescription>, ISchema
        {
            return source.WithDescription(new SchemaDescription(description));
        }
    }
}
