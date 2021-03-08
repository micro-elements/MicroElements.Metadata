// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
}
