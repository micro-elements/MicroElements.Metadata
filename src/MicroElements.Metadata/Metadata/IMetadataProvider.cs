// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public static readonly IEqualityComparer<IProperty> DefaulPropertyComparer = PropertyComparer.ByTypeAndNameComparer;

        /// <summary>
        /// Default search options for metadata providers.
        /// </summary>
        public static readonly SearchOptions DefaulSearchOptions = SearchOptions.Default.WithPropertyComparer(DefaulPropertyComparer);
    }

    /// <summary>
    /// Global metadata cache.
    /// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> to store metadata for objects.
    /// </summary>
    public static class MetadataGlobalCache
    {
        private static readonly ConditionalWeakTable<object, IPropertyContainer> MetadataCache = new ConditionalWeakTable<object, IPropertyContainer>();

        /// <summary>
        /// Gets metadata for <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Source.</param>
        /// <returns>Metadata for instance.</returns>
        public static IPropertyContainer GetInstanceMetadata(this object instance)
        {
            if (instance == null)
                return PropertyContainer.Empty;

            if (!MetadataCache.TryGetValue(instance, out IPropertyContainer propertyList))
            {
                propertyList = new MutablePropertyContainer(searchOptions: MetadataProvider.DefaulSearchOptions);
                MetadataCache.Add(instance, propertyList);
            }

            return propertyList;
        }
    }
}
