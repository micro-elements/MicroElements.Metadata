// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Enum parser.
    /// Parses enum by name or numeric value.
    /// </summary>
    public class EnumUntypedParser : ValueParserBase<object>
    {
        private readonly Type _enumType;
        private readonly bool _allowNull;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUntypedParser"/> class.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <param name="allowNull">Allow null value.</param>
        public EnumUntypedParser(Type enumType, bool allowNull = false)
        {
            _enumType = enumType;
            _allowNull = allowNull;
        }

        /// <inheritdoc />
        public override IParseResult<object> Parse(string? source)
        {
            if (source == null && _allowNull)
                return ParseResult.Success<object>(null);

            if (Enum.TryParse(_enumType, value: source, ignoreCase: true, out object result))
                return ParseResult.Success<object>(result);

            return ParseResult.Failed<object>();
        }
    }
}
