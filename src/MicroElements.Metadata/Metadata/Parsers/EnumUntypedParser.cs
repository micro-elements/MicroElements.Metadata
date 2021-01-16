// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Parsers
{
    /// <summary>
    /// Enum parser.
    /// Parses enum by name.
    /// </summary>
    public class EnumUntypedParser : ValueParserBase<object>
    {
        private readonly Type _enumType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUntypedParser"/> class.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        public EnumUntypedParser(Type enumType)
        {
            _enumType = enumType;
        }

        /// <inheritdoc />
        public override Option<object> Parse(string source)
        {
            if (Enum.TryParse(_enumType, source, true, out object result))
                return result;
            return Option<object>.None;
        }
    }
}
