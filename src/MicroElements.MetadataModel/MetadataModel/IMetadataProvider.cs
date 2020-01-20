// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.MetadataModel
{
    /// <summary>
    /// Represents object that has metadata.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for item.
        /// </summary>
        IReadOnlyDictionary<Type, object> Metadata { get; }
    }

    /// <summary>
    /// Provides extension methods for metadata providers.
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets metadata of some type.
        /// </summary>
        /// <typeparam name="TMetadata">Metadata type.</typeparam>
        /// <param name="metadataProviderHolder">Metadata provider.</param>
        /// <returns>Metadata or default value if not found.</returns>
        public static TMetadata GetMeta<TMetadata>(this IMetadataProvider metadataProviderHolder)
        {
            metadataProviderHolder.AssertArgumentNotNull(nameof(metadataProviderHolder));

            if (metadataProviderHolder.Metadata != null && metadataProviderHolder.Metadata.TryGetValue(typeof(TMetadata), out var value))
            {
                return (TMetadata)value;
            }

            return default;
        }
    }
}
