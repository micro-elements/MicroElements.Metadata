// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Core;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has metadata.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for the current instance.
        /// </summary>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance.</returns>
        IPropertyContainer GetInstanceMetadata(bool autoCreate = false) => MetadataGlobalCacheStorage.Instance.GetInstanceMetadata(this, autoCreate);

        /// <summary>
        /// Replaces metadata for the current instance.
        /// </summary>
        /// <param name="metadata">New metadata.</param>
        void SetInstanceMetadata(IPropertyContainer metadata) => MetadataGlobalCacheStorage.Instance.SetInstanceMetadata(this, metadata);
    }

    /// <summary>
    /// <see cref="IMetadataProvider"/> that implemented with <see cref="Metadata"/> property.
    /// </summary>
    public interface IManualMetadataProvider : IMetadataProvider
    {
        /// <summary>
        /// Gets or sets metadata for the current instance.
        /// </summary>
        IPropertyContainer Metadata { get; }

        /// <inheritdoc />
        IPropertyContainer IMetadataProvider.GetInstanceMetadata(bool autoCreate) => Metadata;

        /// <inheritdoc />
        void IMetadataProvider.SetInstanceMetadata(IPropertyContainer metadata)
        {
            Action<object, IPropertyContainer> propertySetter = ExpressionUtils.GetPropertySetter<IPropertyContainer>(this.GetType(), nameof(Metadata));
            propertySetter.Invoke(this, metadata);
        }
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
