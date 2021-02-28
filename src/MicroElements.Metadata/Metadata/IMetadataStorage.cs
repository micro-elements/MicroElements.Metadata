// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// External metadata storage.
    /// </summary>
    public interface IMetadataStorage
    {
        /// <summary>
        /// Gets or creates metadata for the instance.
        /// </summary>
        /// <param name="instance">The instance to get metadata for.</param>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance or Empty metadata.</returns>
        IPropertyContainer GetInstanceMetadata(object instance, bool autoCreate = true);

        /// <summary>
        /// Replaces metadata for the current instance.
        /// </summary>
        /// <param name="instance">Target instance to set metadata.</param>
        /// <param name="metadata">New metadata.</param>
        void SetInstanceMetadata(object instance, IPropertyContainer metadata);
    }

    /// <summary>
    /// Metadata storage that stores metadata in <see cref="MetadataGlobalCache"/>.
    /// </summary>
    public sealed class MetadataGlobalCacheStorage : IMetadataStorage
    {
        /// <summary>
        /// Gets global storage instance.
        /// </summary>
        public static readonly IMetadataStorage Instance = new MetadataGlobalCacheStorage();

        /// <inheritdoc />
        public IPropertyContainer GetInstanceMetadata(object instance, bool autoCreate = true) =>
            MetadataGlobalCache.GetInstanceMetadata(instance, autoCreate);

        /// <inheritdoc />
        public void SetInstanceMetadata(object instance, IPropertyContainer metadata) =>
            MetadataGlobalCache.SetInstanceMetadata(instance, metadata);
    }
}
