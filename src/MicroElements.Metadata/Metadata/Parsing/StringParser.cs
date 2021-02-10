// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.Parsers;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Default string parser.
    /// </summary>
    public sealed class StringParser : ValueParserBase<string>
    {
        /// <summary>
        /// Gets global static singleton.
        /// </summary>
        public static StringParser Default { get; } = new StringParser();

        /// <summary>
        /// Gets Interned string parser.
        /// </summary>
        public static StringParser Interned { get; } = new StringParser(new InterningStringProvider());

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
}
