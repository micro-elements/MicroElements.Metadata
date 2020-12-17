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
        /// Gets metadata for current instance.
        /// </summary>
        IPropertyContainer Metadata => this.GetInstanceMetadata();

        /// <summary>
        /// Freeze metadata makes metadata readonly.
        /// </summary>
        void FreezeMetadata() => this.FreezeInstanceMetadata();
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
