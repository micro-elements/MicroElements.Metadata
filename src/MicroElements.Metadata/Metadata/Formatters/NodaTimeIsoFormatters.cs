// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Formatters
{
    public class NodaTimeIsoFormatters : IValueFormatterProvider
    {
        public static IValueFormatter LocalDateFormatter =
            new FormattableFormatter("NodaTime.LocalDate", "yyyy-MM-dd");

        public static IValueFormatter LocalTimeFormatter =
            new FormattableFormatter("NodaTime.LocalTime", "HH:mm:ss");

        public static IValueFormatter LocalDateTimeFormatter =
            new FormattableFormatter("NodaTime.LocalDateTime", "yyyy-MM-ddTHH:mm:ss");

        public static IValueFormatter DurationFormatter =
            new FormattableFormatter("NodaTime.Duration", "-H:mm:ss.FFFFFFFFF");

        public static IValueFormatter DurationWithDaysFormatter =
            new FormattableFormatter("NodaTime.Duration", "-D:hh:mm:ss.FFFFFFFFF");

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
