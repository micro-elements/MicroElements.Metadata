// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Marker metadata for collections.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface ICollectionMetadata : IMetadata
    {
    }

    /// <summary>
    /// Provides metadata to describe minimum allowed item count for collection.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IMinItems : ICollectionMetadata
    {
        /// <summary>
        /// Gets minimum allowed item count for collection.
        /// </summary>
        int Value { get; }
    }

    /// <summary>
    /// Provides metadata to describe maximum allowed item count for collection.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IMaxItems : ICollectionMetadata
    {
        /// <summary>
        /// Gets maximum allowed item count for collection.
        /// </summary>
        int Value { get; }
    }
}
