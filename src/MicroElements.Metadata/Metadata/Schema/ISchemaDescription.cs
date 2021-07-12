// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides description for schema, property.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.AnySchema)]
    public interface ISchemaDescription : IMetadata
    {
        /// <summary>
        /// Gets description.
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// Provides description for schema, property.
    /// </summary>
    public record SchemaDescription : ISchemaDescription
    {
        /// <inheritdoc />
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDescription"/> class.
        /// </summary>
        /// <param name="description">Description for scheme.</param>
        /// <exception cref="ArgumentNullException"><paramref name="description"/> is null.</exception>
        public SchemaDescription(string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}
