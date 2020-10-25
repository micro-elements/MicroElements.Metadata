// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Formatting extensions.
    /// </summary>
    internal static class StringFormatter
    {
        /// <summary>
        /// Default string formatting for most used types.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatValue(this object? value)
        {
            if (value == null)
                return "null";

            if (value is string stringValue)
                return stringValue;

            if (value.GetType().FullName == "NodaTime.LocalDate" && value is IFormattable localDate)
                return localDate.ToString("yyyy-MM-dd", null);

            if ((value.GetType().FullName == "NodaTime.LocalDateTime" || value is DateTime) && value is IFormattable localDateTime)
                return localDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFF", null);

            return Functional.StringFormatter.DefaultFormatValue(value);
        }
    }
}
