// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Default string parser.
    /// </summary>
    public class StringParser : ValueParserBase<string>
    {
        /// <summary>
        /// Global static singleton.
        /// </summary>
        public static readonly StringParser Instance = new StringParser();

        /// <inheritdoc />
        public override ParseResult<string> Parse(string? source) => ParseResult.Success(source!);
    }
}
