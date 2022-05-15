// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Collections.TwoLayerCache;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property factory that caches properties.
    /// </summary>
    public class CachedPropertyFactory : IPropertyFactory
    {
        /// <summary>
        /// Default max size for cache.
        /// </summary>
        public const int DefaultCacheMaxItemsCount = 256;

        private readonly IPropertyFactory _propertyFactory;
        private readonly TwoLayerCache<(Type Type, string Name), IProperty> _propertiesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPropertyFactory"/> class.
        /// </summary>
        /// <param name="propertyFactory">Property factory that creates properties.</param>
        /// <param name="maxItemCount">If maxItemCount is set that <see cref="TwoLayerCache{TKey,TValue}"/> uses for caching.</param>
        public CachedPropertyFactory(
            IPropertyFactory? propertyFactory = null,
            int maxItemCount = DefaultCacheMaxItemsCount)
        {
            _propertyFactory = propertyFactory ?? new PropertyFactory();
            _propertiesCache = new TwoLayerCache<(Type, string), IProperty>(maxItemCount);
        }

        /// <inheritdoc />
        public IProperty<T> Create<T>(string name)
        {
            return (IProperty<T>)_propertiesCache.GetOrAdd(
                (typeof(T), name),
                (node, factory) => factory.Create<T>(node.Name),
                _propertyFactory);
        }

        /// <inheritdoc />
        public IProperty Create(Type type, string name)
        {
            return _propertiesCache.GetOrAdd(
                (type, name),
                (node, factory) => factory.Create(node.Type, node.Name),
                _propertyFactory);
        }
    }

    /// <summary>
    /// Extensions for <see cref="IPropertyFactory"/>.
    /// </summary>
    public static partial class PropertyFactoryExtensions
    {
        /// <summary>
        /// Creates factory that caches <see cref="IProperty"/> for the same property type and name.
        /// </summary>
        /// <param name="propertyFactory">Factory.</param>
        /// <param name="maxItemCount">Max item count in cache.</param>
        /// <returns>New cached <see cref="IPropertyFactory"/>.</returns>
        public static IPropertyFactory Cached(this IPropertyFactory propertyFactory, int maxItemCount = CachedPropertyFactory.DefaultCacheMaxItemsCount)
        {
            return new CachedPropertyFactory(propertyFactory,  maxItemCount);
        }
    }
}
