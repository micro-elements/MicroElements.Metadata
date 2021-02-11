// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// String parser.
    /// </summary>
    public sealed partial class StringParser : ValueParserBase<string>
    {
        private readonly IStringProvider _stringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringParser"/> class.
        /// </summary>
        /// <param name="stringProvider">Optional string provider.</param>
        public StringParser(IStringProvider? stringProvider = null)
        {
            _stringProvider = stringProvider ?? new DefaultStringProvider();
        }

        /// <inheritdoc />
        public override ParseResult<string> Parse(string? source)
        {
            return source == null ? ParseResult<string>.Empty : ParseResult.Success(_stringProvider.GetString(source));
        }
    }

    /// <summary>
    /// String parser.
    /// </summary>
    public partial class StringParser
    {
        /// <summary>
        /// Gets global static singleton.
        /// </summary>
        public static StringParser Default { get; } = new StringParser();

        /// <summary>
        /// Gets Interned string parser.
        /// </summary>
        public static StringParser Interned { get; } = new StringParser(new InterningStringProvider());

        /// <summary>
        /// Gets cached string provider.
        /// </summary>
        /// <param name="cachedStringProvider">Optional cached string provider.</param>
        /// <returns>New <see cref="StringParser"/> instance.</returns>
        public static StringParser Cached(CachedStringProvider? cachedStringProvider = null) => new StringParser(cachedStringProvider ?? new CachedStringProvider());
    }
}
