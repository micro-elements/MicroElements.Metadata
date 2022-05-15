// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Collections.TwoLayerCache;

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
        string? Description { get; }
    }

    /// <summary>
    /// Provides description for schema, property.
    /// </summary>
    public partial record SchemaDescription(string? Description) : ISchemaDescription;

    public partial record SchemaDescription
    {
        /// <summary>
        /// Gets cached <see cref="ISchemaDescription"/>.
        /// </summary>
        /// <param name="description">Description.</param>
        /// <returns>Cached description.</returns>
        public static ISchemaDescription FromStringCached(string description)
        {
            return TwoLayerCache
                .Instance<string, ISchemaDescription>(nameof(ISchemaDescription))
                .GetOrAdd(description, s => new SchemaDescription(s));
        }
    }
}
