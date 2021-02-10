// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace MicroElements.Metadata
{
    /// <summary>
    /// String provider to support string interning or caching for performance needs.
    /// </summary>
    public interface IStringProvider
    {
        /// <summary>
        /// Gets string instance.
        /// </summary>
        /// <param name="value">Source value.</param>
        /// <returns>Result value.</returns>
        string GetString(string value);
    }

    /// <summary>
    /// Default string provider that returns the same string.
    /// </summary>
    public sealed class DefaultStringProvider : IStringProvider
    {
        /// <inheritdoc />
        public string GetString(string value) => value;
    }

    /// <summary>
    /// String provider that returns interned string.
    /// </summary>
    public sealed class InterningStringProvider : IStringProvider
    {
        /// <inheritdoc />
        public string GetString(string value) => string.Intern(value);
    }

    /// <summary>
    /// String provider that returns cached string instance.
    /// </summary>
    public sealed class CachedStringProvider : IStringProvider
    {
        // Internal string cache.
        private readonly ConcurrentDictionary<string, string> _strings = new ConcurrentDictionary<string, string>();

        /// <inheritdoc />
        public string GetString(string value) => _strings.GetOrAdd(value, s => s);
    }
}
