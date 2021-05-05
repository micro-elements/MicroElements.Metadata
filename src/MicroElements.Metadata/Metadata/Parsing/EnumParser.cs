// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Enum parser.
    /// Parses enum by name or numeric value.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    public class EnumParser<TEnum> : ValueParserBase<TEnum>
    {
        /// <inheritdoc />
        public override IParseResult<TEnum> Parse(string? source)
        {
            if (Enum.TryParse(typeof(TEnum), value: source, ignoreCase: true, out object result))
                return ParseResult.Success((TEnum)result);
            return ParseResult.Failed<TEnum>();
        }
    }
}
