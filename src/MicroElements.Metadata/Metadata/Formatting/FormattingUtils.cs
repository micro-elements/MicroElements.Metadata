// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Text;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatting extensions.
    /// </summary>
    public static class FormattingUtils
    {
        /// <summary>
        /// Formats enumeration of value as tuple: (value1, value2, ...).
        /// </summary>
        /// <param name="values">Values enumeration.</param>
        /// <param name="fieldSeparator">Optional field separator.</param>
        /// <param name="nullPlaceholder">Optional null placeholder.</param>
        /// <param name="startSymbol">Start symbol. DefaultValue='('.</param>
        /// <param name="endSymbol">End symbol. DefaultValue=')'.</param>
        /// <param name="formatValue">Func to format object value to string representation.</param>
        /// <param name="maxItems">Optional max items to render.</param>
        /// <param name="maxTextLength">Limits max text length.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatAsTuple(
            this IEnumerable values,
            string fieldSeparator = ", ",
            string nullPlaceholder = "null",
            string startSymbol = "(",
            string endSymbol = ")",
            Func<object, string>? formatValue = null,
            int? maxItems = null,
            int maxTextLength = 1028)
        {
            values.AssertArgumentNotNull(nameof(values));
            fieldSeparator.AssertArgumentNotNull(nameof(fieldSeparator));
            nullPlaceholder.AssertArgumentNotNull(nameof(nullPlaceholder));
            startSymbol.AssertArgumentNotNull(nameof(startSymbol));
            endSymbol.AssertArgumentNotNull(nameof(endSymbol));

            formatValue ??= value => value.FormatValue(nullPlaceholder: nullPlaceholder);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(startSymbol);
            int count = 1;
            foreach (var value in values)
            {
                if (stringBuilder.Length > maxTextLength || (maxItems.HasValue && count > maxItems.Value))
                    break;

                string text = value != null ? formatValue(value) : nullPlaceholder;
                stringBuilder.Append($"{text}{fieldSeparator}");

                count++;
            }

            if (stringBuilder.Length > maxTextLength || (maxItems.HasValue && count > maxItems.Value))
                stringBuilder.Append($"...{fieldSeparator}");

            if (stringBuilder.Length > fieldSeparator.Length)
                stringBuilder.Length -= fieldSeparator.Length;

            stringBuilder.Append(endSymbol);
            return stringBuilder.ToString();
        }
    }
}
