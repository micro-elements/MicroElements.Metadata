// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;

namespace MicroElements.Core
{
    /// <summary>
    /// Simple in memory cache adapter.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// Gets or adds value from (to) cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
    }

    /// <summary>
    /// Simple in memory cache adapter.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public class CacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly Func<TKey, Func<TKey, TValue>, TValue> _getOrAdd;

        public CacheAdapter(Func<TKey, Func<TKey, TValue>, TValue> getOrAdd)
        {
            _getOrAdd = getOrAdd;
        }

        /// <inheritdoc />
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return _getOrAdd(key, valueFactory);
        }
    }

    public class ConcurrentDictionaryAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> _values;

        public ConcurrentDictionaryAdapter(ConcurrentDictionary<TKey, TValue> values)
        {
            _values = values;
        }

        /// <inheritdoc />
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return _values.GetOrAdd(key, valueFactory);
        }
    }
}
