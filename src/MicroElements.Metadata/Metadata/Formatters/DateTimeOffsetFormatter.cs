using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Formats <see cref="DateTimeOffset"/> in ISO-8601.
    /// Uses format "yyyy-MM-ddTHH:mm:ss.FFF" for dates with time and "yyyy-MM-dd" for dates without time.
    /// </summary>
    public sealed class DateTimeOffsetFormatter : IValueFormatter<DateTimeOffset>
    {
        /// <summary>
        /// Formats as 'yyyy-MM-ddTHH:mm:ss.fffK'.
        /// </summary>
        public static IValueFormatter<DateTimeOffset> Iso { get; } = new DateTimeOffsetFormatter("yyyy-MM-ddTHH:mm:ss.fffK");

        /// <summary>
        /// Ignores Offset and formats only data and time part.
        /// </summary>
        public static IValueFormatter<DateTimeOffset> IgnoringOffset { get; } = new DateTimeOffsetFormatter("yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-dd");

        /// <summary>
        /// Gets full format.
        /// </summary>
        public string FullFormat { get; }

        /// <summary>
        /// Gets format that uses when data contains only date without time.
        /// </summary>
        public string DateOnlyFormat { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetFormatter"/> class.
        /// </summary>
        /// <param name="fullFormat">Full format.</param>
        /// <param name="dateOnlyFormat">Format that uses when data contains only date without time.</param>
        public DateTimeOffsetFormatter(string fullFormat, string? dateOnlyFormat = null)
        {
            fullFormat.AssertArgumentNotNull(nameof(fullFormat));

            FullFormat = fullFormat;
            DateOnlyFormat = dateOnlyFormat ?? fullFormat;
        }

        /// <inheritdoc />
        public string Format(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.TimeOfDay.Ticks > 0
                ? dateTimeOffset.ToString(FullFormat)
                : dateTimeOffset.ToString(DateOnlyFormat);
        }
    }
}
