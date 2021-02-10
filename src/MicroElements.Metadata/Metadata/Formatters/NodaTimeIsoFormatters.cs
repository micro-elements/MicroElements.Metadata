// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// NodaTime formatters.
    /// </summary>
    public class NodaTimeIsoFormatters : IValueFormatterProvider
    {
        /// <summary>
        /// Formats LocalDate as yyyy-MM-dd.
        /// </summary>
        public static IValueFormatter LocalDateFormatter { get; } = new FormattableFormatter("NodaTime.LocalDate", "yyyy-MM-dd");

        /// <summary>
        /// Formats LocalTime as HH:mm:ss.
        /// </summary>
        public static IValueFormatter LocalTimeFormatter { get; } = new FormattableFormatter("NodaTime.LocalTime", "HH:mm:ss");

        /// <summary>
        /// Formats LocalDateTime as yyyy-MM-ddTHH:mm:ss.
        /// </summary>
        public static IValueFormatter LocalDateTimeFormatter { get; } = new FormattableFormatter("NodaTime.LocalDateTime", "yyyy-MM-ddTHH:mm:ss.FFF");

        /// <summary>
        /// Formats Duration as -H:mm:ss.FFFFFFFFF (without days).
        /// </summary>
        public static IValueFormatter DurationFormatter { get; } = new FormattableFormatter("NodaTime.Duration", "-H:mm:ss.FFF");

        /// <summary>
        /// Formats Duration as -D:hh:mm:ss.FFFFFFFFF (with days).
        /// </summary>
        public static IValueFormatter DurationWithDaysFormatter { get; } = new FormattableFormatter("NodaTime.Duration", "-D:hh:mm:ss.FFF");

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            yield return LocalDateFormatter;
            yield return LocalTimeFormatter;
            yield return LocalDateTimeFormatter;
            yield return DurationFormatter;
        }
    }
}
