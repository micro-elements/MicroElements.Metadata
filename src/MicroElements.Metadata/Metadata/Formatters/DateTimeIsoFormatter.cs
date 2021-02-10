// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Formats <see cref="DateTime"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss" for dates with time and "yyyy-MM-dd" for dates without time.
    /// </summary>
    public class DateTimeIsoFormatter : IValueFormatter<DateTime>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<DateTime> Instance { get; } = new DateTimeIsoFormatter();

        /// <inheritdoc />
        public string Format(DateTime dateTime)
        {
            return dateTime.TimeOfDay.Ticks > 0 ? $"{dateTime:yyyy-MM-ddTHH:mm:ss.FFF}" : $"{dateTime:yyyy-MM-dd}";
        }
    }
}
