// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formats <see cref="DateTime"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss.FFF" for dates with time and "yyyy-MM-dd" for dates without time.
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

    /// <summary>
    /// Formats <see cref="DateTimeOffset"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss.FFF" for dates with time and "yyyy-MM-dd" for dates without time.
    /// </summary>
    public class DateTimeOffsetIsoFormatter : IValueFormatter<DateTimeOffset>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<DateTimeOffset> Instance { get; } = new DateTimeOffsetIsoFormatter();

        /// <inheritdoc />
        public string Format(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.TimeOfDay.Ticks > 0 ? $"{dateTimeOffset:yyyy-MM-ddTHH:mm:ss.FFF}" : $"{dateTimeOffset:yyyy-MM-dd}";
        }
    }

    /// <summary>
    /// Formats <see cref="TimeSpan"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss" for dates with time and "yyyy-MM-dd" for dates without time.
    /// </summary>
    public class TimeSpanFormatter : IValueFormatter<TimeSpan>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<TimeSpan> Instance { get; } = new TimeSpanFormatter();

        /// <inheritdoc />
        public string Format(TimeSpan dateTime)
        {
            return dateTime.ToString("g");
        }
    }
}
