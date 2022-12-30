using System;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formats <see cref="TimeSpan"/> in various formats.
    /// </summary>
    public class TimeSpanFormatter : IValueFormatter<TimeSpan>
    {
        /// <summary>
        /// Gets Constant (invariant) format (not ISO-8601).
        /// Format: <c>[-][d.]hh:mm:ss[.fffffff]</c>
        /// Example: 1.01:57:54
        /// </summary>
        public static IValueFormatter<TimeSpan> Invariant { get; } = new TimeSpanFormatter("c");

        /// <summary> Gets full format. </summary>
        public string TimeSpanFormat { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanFormatter"/> class.
        /// </summary>
        /// <param name="timeSpanFormat">Format.</param>
        public TimeSpanFormatter(string timeSpanFormat)
        {
            TimeSpanFormat = timeSpanFormat;
        }

        /// <inheritdoc />
        public string Format(TimeSpan dateTime)
        {
            return dateTime.ToString(TimeSpanFormat);
        }
    }
}
