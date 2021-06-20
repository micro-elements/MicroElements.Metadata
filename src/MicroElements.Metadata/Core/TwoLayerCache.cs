// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Core
{
    // TODO: Move TwoLayerCache to Shared
    /// <summary>
    /// Represents cache that holds only limited number of items.
    /// Cache organized in two layers: hot and cold. Items first added to cold cache.
    /// GetValue first checks hot cache. If value not found in hot cache than cold cache uses for search.
    /// If value exists in cold cache than item moves to hot cache.
    /// If hot cache exceeds item limit then hot cache became cold cache and new hot cache creates.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public class TwoLayerCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly int _maxItemCount;
        private readonly object _sync = new ();
        private readonly IEqualityComparer<TKey> _comparer;
        private ConcurrentDictionary<TKey, TValue> _hotCache;
        private ConcurrentDictionary<TKey, TValue> _coldCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoLayerCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="maxItemCount">Max item count.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
        public TwoLayerCache(int maxItemCount, IEqualityComparer<TKey>? comparer = null)
        {
            if (maxItemCount <= 0)
                throw new ArgumentException($"maxItemCount should be non negative number but was {maxItemCount}");

            _maxItemCount = maxItemCount;
            _comparer = comparer!;
            _hotCache = CreateCache();
            _coldCache = CreateCache();
        }

        private ConcurrentDictionary<TKey, TValue> CreateCache() => new ConcurrentDictionary<TKey, TValue>(_comparer);

        /// <summary>
        /// Attempts to add the specified key and value to internal cold cache.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>true if the key/value pair was added.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            // Add items only to cold cache.
            return _coldCache.TryAdd(key, value);
        }

        /// <summary>
        /// Gets item by key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">Found value or default.</param>
        /// <returns>true if value found by key.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            // Try get value from hot cache.
            if (_hotCache.TryGetValue(key, out value))
            {
                // Cache hit, no more actions.
                return true;
            }

            // Check whether value exists in cold cache.
            if (_coldCache.TryGetValue(key, out value))
            {
                // Value exists in cold cache so move to hot cache.
                _hotCache.TryAdd(key, value);

                // If not remove from cold then cold cache size can be twice as hot.
                // Remove omitted for performance reason.
                // _coldCache.TryRemove(key, out _);

                // If hot cache exceeds limit then move all to cold cache and create new hot cache.
                if (_hotCache.Count > _maxItemCount)
                {
                    lock (_sync)
                    {
                        if (_hotCache.Count > _maxItemCount)
                        {
                            _coldCache = _hotCache;
                            _hotCache = CreateCache();
                        }
                    }
                }

                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets or adds value from (to) cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            // Try get value from hot cache.
            if (TryGetValue(key, out TValue value))
            {
                // Cache hit, no more actions.
                return value;
            }

            return _coldCache.GetOrAdd(key, valueFactory);
        }
    }
}
