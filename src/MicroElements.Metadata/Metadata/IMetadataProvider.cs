// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has metadata.
    /// Has default interface implementation of <see cref="Metadata"/> as <see cref="MetadataGlobalCache.GetInstanceMetadata"/>.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for the current instance.
        /// </summary>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance.</returns>
        IPropertyContainer GetMetadata(bool autoCreate = true) => this.GetInstanceMetadata(autoCreate);

        /// <summary>
        /// Replaces metadata for the current instance.
        /// </summary>
        /// <param name="metadata">New metadata.</param>
        void SetMetadata(IPropertyContainer metadata) => this.SetInstanceMetadata(metadata);

        /// <summary>
        /// Gets metadata for the current instance.
        /// </summary>
        IPropertyContainer Metadata => GetMetadata(autoCreate: true);
    }

    /// <summary>
    /// MetadataProvider statics.
    /// </summary>
    public static class MetadataProvider
    {
        /// <summary>
        /// Gets default property comparer for metadata search.
        /// </summary>
        public static IEqualityComparer<IProperty> DefaultMetadataComparer { get; } = PropertyComparer.ByTypeAndNameIgnoreCaseComparer;

        /// <summary>
        /// Gets default search options for metadata providers.
        /// </summary>
        public static SearchOptions DefaultSearchOptions { get; } = SearchOptions.Default.WithPropertyComparer(DefaultMetadataComparer);
    }
}
