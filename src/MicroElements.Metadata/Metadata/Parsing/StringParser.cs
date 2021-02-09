// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Default string parser.
    /// </summary>
    public sealed class StringParser : ValueParserBase<string>
    {
        /// <summary>
        /// Gets global static singleton.
        /// </summary>
        public static StringParser Instance { get; } = new StringParser();

        /// <inheritdoc />
        public override ParseResult<string> Parse(string? source) => source == null ? ParseResult<string>.Empty : ParseResult.Success<string>(source);
    }

    /// <summary>
    /// String parser that interns every string.
    /// </summary>
    public sealed class InternedStringParser : ValueParserBase<string>
    {
        /// <summary>
        /// Gets global static singleton.
        /// </summary>
        public static InternedStringParser Instance { get; } = new InternedStringParser();

        /// <inheritdoc />
        public override ParseResult<string> Parse(string? source) => source == null ? ParseResult<string>.Empty : ParseResult.Success(string.Intern(source));
    }

    /// <summary>
    /// String parser that caches every string.
    /// </summary>
    public sealed class CachedStringParser : ValueParserBase<string>
    {
        /// <summary>
        /// Gets global static singleton.
        /// </summary>
        public static CachedStringParser Instance { get; } = new CachedStringParser();

        private readonly ConcurrentDictionary<string, string> _strings = new ConcurrentDictionary<string, string>();

        /// <inheritdoc />
        public override ParseResult<string> Parse(string? source) =>
            source == null ? ParseResult<string>.Empty : ParseResult.Success(_strings.GetOrAdd(source, s => s));
    }
}
