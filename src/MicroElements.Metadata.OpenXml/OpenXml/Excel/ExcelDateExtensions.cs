// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using NodaTime;

namespace MicroElements.Metadata.OpenXml.Excel
{
    /// <summary>
    /// Dates convert functions.
    /// <para>
    /// In Excel, the base date that is used is 01/01/1900 and the year 1900 is treated as a leap year.
    /// In OLE automation, Microsoft corrected this by using the base date of 12/31/1899.
    /// See: https://stackoverflow.com/questions/727466/how-do-i-convert-an-excel-serial-date-number-to-a-net-datetime.
    /// See: https://stackoverflow.com/questions/39627749/adding-a-date-in-an-excel-cell-using-openxml
    /// </para>
    /// </summary>
    public static class ExcelDateExtensions
    {
        public static readonly DateTime date_1899_12_31 = new DateTime(1899, 12, 31);

        /// <summary>
        /// Excel date and time has millisecond precision.
        /// Also: <see cref="DateTime.ToOADate"/> and <see cref="DateTime.FromOADate"/> has millisecond precision.
        /// Also: Excel can hold only 15 digits: https://en.wikipedia.org/wiki/Numeric_precision_in_Microsoft_Excel.
        /// </summary>
        public static DateTime TrimToMillisecondPrecision(this in DateTime dateTime)
        {
            long overhead = dateTime.Ticks % TimeSpan.TicksPerMillisecond;
            if (overhead > 0)
                return dateTime.AddTicks(-overhead);

            return dateTime;
        }

        /// <summary>
        /// Converts DateTime to Excel serial date (Days since 1900 year).
        /// It's an alternative for <see cref="DateTime.ToOADate"/> that fixes 1900 leap (not leap) year problem.
        /// </summary>
        /// <param name="dateTime">Source date time.</param>
        /// <returns>Double time.</returns>
        public static double ToExcelSerialDate(this in DateTime dateTime)
        {
            double excelSerialDate = (dateTime.TrimToMillisecondPrecision() - date_1899_12_31).TotalDays;
            if (excelSerialDate > 59)
                excelSerialDate += 1;
            return excelSerialDate;
        }

        /// <summary>
        /// Converts Excel Serial Date to DateTime.
        /// </summary>
        /// <param name="excelSerialDate">Excel Serial Date.</param>
        /// <returns>DateTime.</returns>
        public static DateTime FromExcelSerialDate(this double excelSerialDate)
        {
            // NOTE: DateTime.FromOADate parses 1 as 1899-12-31. Correct value is 1900-01-01
            if (excelSerialDate > 59)
                excelSerialDate -= 1; // Excel/Lotus 2/29/1900 bug
            return date_1899_12_31.AddDays(excelSerialDate).TrimToMillisecondPrecision();
        }

        /// <summary>
        /// Converts different date related types to Excel Serial Date.
        /// Can convert: <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref=""/>
        /// </summary>
        /// <param name="valueUntyped">Boxed date related value.</param>
        /// <returns>Excel Serial Date as string.</returns>
        public static double ToExcelSerialDate(this object? valueUntyped)
        {
            double excelSerialDate = valueUntyped switch
            {
                DateTime dateTime => dateTime.ToExcelSerialDate(),
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime.ToExcelSerialDate(),
                LocalDate localDate => localDate.ToDateTimeUnspecified().ToExcelSerialDate(),
                LocalDateTime localDateTime => localDateTime.ToDateTimeUnspecified().ToExcelSerialDate(),
                LocalTime localTime => Duration.FromNanoseconds(localTime.NanosecondOfDay).TotalHours / 24.0,
                Duration duration => duration.TotalHours / 24.0,
                TimeSpan timeSpan => timeSpan.TotalHours / 24.0,
                _ => 1, // 1900-01-01
            };

            return excelSerialDate;
        }

        /// <summary>
        /// Converts different date related types to Excel Serial Date.
        /// </summary>
        /// <param name="valueUntyped">Boxed date related value.</param>
        /// <returns>Excel Serial Date as string.</returns>
        public static string? ToExcelSerialDateAsString(this object? valueUntyped)
        {
            return valueUntyped?
                .ToExcelSerialDate()
                .ToString(CultureInfo.InvariantCulture);
        }
    }
}
