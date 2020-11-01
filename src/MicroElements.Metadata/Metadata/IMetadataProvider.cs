// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has metadata.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for current instance.
        /// </summary>
        IPropertyContainer Metadata => this.GetInstanceMetadata();
    }

    /// <summary>
    /// MetadataProvider statics.
    /// </summary>
    public static class MetadataProvider
    {
        /// <summary>
        /// Default property comparer for metadata search.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> DefaultMetadataComparer = PropertyComparer.ByTypeAndNameComparer;

        /// <summary>
        /// Default search options for metadata providers.
        /// </summary>
        public static readonly SearchOptions DefaultSearchOptions = SearchOptions.Default.WithPropertyComparer(DefaultMetadataComparer);
    }
}
