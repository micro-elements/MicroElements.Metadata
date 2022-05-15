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
        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            public readonly Type Type;
            public readonly string Name;
            public readonly IPropertyFactory PropertyFactory;

            public CacheKey(Type type, string name, IPropertyFactory propertyFactory)
            {
                Type = type;
                Name = name;
                PropertyFactory = propertyFactory;
            }

            /// <inheritdoc />
            public bool Equals(CacheKey other)
            {
                return Type == other.Type && Name == other.Name;
            }

            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Type, Name);
        }

        private readonly IPropertyFactory _propertyFactory;
        private readonly TwoLayerCache<CacheKey, IProperty> _propertyValuesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPropertyFactory"/> class.
        /// </summary>
        /// <param name="propertyFactory">Property factory that creates properties.</param>
        /// <param name="maxItemCount">If maxItemCount is set that <see cref="TwoLayerCache{TKey,TValue}"/> uses for caching.</param>
        public CachedPropertyFactory(
            IPropertyFactory? propertyFactory = null,
            int? maxItemCount = null)
        {
            _propertyFactory = propertyFactory ?? new PropertyFactory();
            _propertyValuesCache = new TwoLayerCache<CacheKey, IProperty>(maxItemCount.GetValueOrDefault(4000));
        }

        /// <inheritdoc />
        public IProperty<T> Create<T>(string name)
        {
            return (IProperty<T>)_propertyValuesCache.GetOrAdd(
                new CacheKey(typeof(T), name, _propertyFactory),
                node => node.PropertyFactory.Create<T>(node.Name));
        }

        /// <inheritdoc />
        public IProperty Create(Type type, string name)
        {
            return _propertyValuesCache.GetOrAdd(
                new CacheKey(type, name, _propertyFactory),
                node => node.PropertyFactory.Create(node.Type, node.Name));
        }
    }

    /// <summary>
    /// Extensions for <see cref="IPropertyFactory"/>.
    /// </summary>
    public static class PropertyFactoryExtensions
    {
        /// <summary>
        /// Creates factory that caches <see cref="IProperty"/> for the same property type and name.
        /// </summary>
        /// <param name="propertyFactory">Factory.</param>
        /// <param name="maxItemCount">Max item count in cache.</param>
        /// <returns>New cached <see cref="IPropertyFactory"/>.</returns>
        public static IPropertyFactory Cached(this IPropertyFactory propertyFactory, int? maxItemCount = null)
        {
            return new CachedPropertyFactory(propertyFactory, maxItemCount: maxItemCount);
        }
    }
}
