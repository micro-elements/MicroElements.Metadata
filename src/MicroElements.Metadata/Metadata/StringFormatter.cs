using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    public static class StringFormatter
    {
        /// <summary>
        /// Formats enumeration of value as tuple: (value1, value2, ...).
        /// </summary>
        /// <param name="values">Values enumeration.</param>
        /// <param name="fieldSeparator">Optional field separator.</param>
        /// <param name="nullPlaceholder">Optional null placeholder.</param>
        /// <param name="formatValue">Func to format object value to string representation.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatAsTuple(
            this IEnumerable values,
            string fieldSeparator = ", ",
            string nullPlaceholder = "null",
            Func<object, string> formatValue = null)
        {
            values.AssertArgumentNotNull(nameof(values));
            fieldSeparator.AssertArgumentNotNull(nameof(fieldSeparator));
            nullPlaceholder.AssertArgumentNotNull(nameof(nullPlaceholder));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            foreach (var value in values)
            {
                string text = value != null ? formatValue != null ? formatValue(value) : value.ToString() : nullPlaceholder;
                stringBuilder.Append($"{text}{fieldSeparator}");
            }
            if (stringBuilder.Length > fieldSeparator.Length)
                stringBuilder.Length -= fieldSeparator.Length;
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        public static string FormatValue(this object value)
        {
            if (value == null)
                return "null";

            if (value.GetType().FullName == "NodaTime.LocalDate" && value is IFormattable localDate)
                return localDate.ToString("yyyy-MM-dd", null);

            if ((value.GetType().FullName == "NodaTime.LocalDateTime" || value is DateTime) && value is IFormattable localDateTime)
                return localDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFF", null);

            return value.ToString();
        }

        public static string FormatList<T>(this IEnumerable<T> list)
        {
            return list.FormatAsTuple(formatValue: FormatValue);
        }
    }
}
