// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Formats <see cref="DateTime"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss.FFF" for dates with time and "yyyy-MM-dd" for dates without time.
    /// </summary>
    public sealed class DateTimeFormatter : IValueFormatter<DateTime>
    {
        /// <summary>
        /// Formats as 'yyyy-MM-ddTHH:mm:ss.FFF' or 'yyyy-MM-dd' (for dates only).
        /// </summary>
        public static IValueFormatter<DateTime> IsoShort { get; } = new DateTimeFormatter("yyyy-MM-ddTHH:mm:ss.FFF", "yyyy-MM-dd");

        /// <summary>
        /// Gets full format.
        /// </summary>
        public string FullFormat { get; }

        /// <summary>
        /// Gets format that uses when data contains only date without time.
        /// </summary>
        public string DateOnlyFormat { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFormatter"/> class.
        /// </summary>
        /// <param name="fullFormat">Full format.</param>
        /// <param name="dateOnlyFormat">Format that uses when data contains only date without time.</param>
        public DateTimeFormatter(string fullFormat, string? dateOnlyFormat = null)
        {
            fullFormat.AssertArgumentNotNull(nameof(fullFormat));

            FullFormat = fullFormat;
            DateOnlyFormat = dateOnlyFormat ?? fullFormat;
        }

        /// <inheritdoc />
        public string Format(DateTime dateTime)
        {
            return dateTime.TimeOfDay.Ticks > 0
                ? dateTime.ToString(FullFormat)
                : dateTime.ToString(DateOnlyFormat);
        }
    }
}
