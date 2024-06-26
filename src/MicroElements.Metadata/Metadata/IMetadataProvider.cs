﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Collections.Cache;
using MicroElements.Reflection.Expressions;

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
        IPropertyContainer GetMetadataContainer(bool autoCreate = false) => MetadataGlobalCacheStorage.Instance.GetInstanceMetadata(this, autoCreate);

        /// <summary>
        /// Replaces metadata for the current instance.
        /// </summary>
        /// <param name="metadata">New metadata.</param>
        void SetMetadataContainer(IPropertyContainer metadata) => MetadataGlobalCacheStorage.Instance.SetInstanceMetadata(this, metadata);
    }

    /// <summary>
    /// <see cref="IMetadataProvider"/> that implemented with <see cref="Metadata"/> property.
    /// </summary>
    public interface IManualMetadataProvider : IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for the current instance.
        /// </summary>
        IPropertyContainer Metadata { get; }

        /// <inheritdoc />
        IPropertyContainer IMetadataProvider.GetMetadataContainer(bool autoCreate)
        {
            if (autoCreate && Metadata == null!)
            {
                SetMetadataContainer(MetadataGlobalCacheStorage.Instance.GetInstanceMetadata(this, autoCreate: true));
            }

            return Metadata ?? PropertyContainer.Empty;
        }

        /// <inheritdoc />
        void IMetadataProvider.SetMetadataContainer(IPropertyContainer metadata)
        {
            Type realType = this.GetType();
            Action<object, IPropertyContainer> propertySetter = Cache
                .Instance<Type, Action<object, IPropertyContainer>>()
                .GetOrAdd(realType, type => Expressions.GetPropertySetter<IPropertyContainer>(type, nameof(Metadata)));
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
        public static IEqualityComparer<IProperty> DefaultMetadataComparer { get; } = PropertyComparer.DefaultMetadataComparer;

        /// <summary>
        /// Gets default search options for metadata providers.
        /// </summary>
        public static SearchOptions DefaultSearchOptions { get; } = SearchOptions.Default.WithPropertyComparer(DefaultMetadataComparer);

        /// <summary>
        /// Creates new property container with default metadata search options.
        /// </summary>
        /// <param name="concurrent">Use concurrent version.</param>
        /// <returns>New instance.</returns>
        public static IMutablePropertyContainer CreateMutableContainer(bool concurrent = false)
        {
            return concurrent
                ? new ConcurrentMutablePropertyContainer(searchOptions: DefaultSearchOptions)
                : new MutablePropertyContainer(searchOptions: DefaultSearchOptions);
        }
    }
}
