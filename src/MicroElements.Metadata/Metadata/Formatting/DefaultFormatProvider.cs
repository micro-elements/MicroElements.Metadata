// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Default format provider.
    /// Provides:
    /// - string formatting;
    /// - floating numbers formatting (float, double, decimal);
    /// - DateTime, DateTimeOffset, TimeSpan formatters;
    /// - NodaTime formatters;
    /// - Type formatter.
    /// </summary>
    public class DefaultFormatProvider : IValueFormatterProvider
    {
        /// <summary>
        /// Gets default format provider instance.
        /// </summary>
        public static DefaultFormatProvider Instance { get; } = new DefaultFormatProvider();

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            // String
            yield return SimpleStringFormatter.Instance;

            // Floating numbers (integers can be formatted default formatter)
            yield return FloatFormatter.Instance;
            yield return DoubleFormatter.Instance;
            yield return DecimalFormatter.Instance;

            // Date and Time
            yield return DateTimeFormatter.IsoShort;
            yield return DateTimeOffsetFormatter.Iso;
            yield return TimeSpanFormatter.Invariant;

            // NodaTime Formatters
            foreach (var nodaTimeFormatter in new NodaTimeIsoFormatters().GetFormatters())
            {
                yield return nodaTimeFormatter;
            }

            // Type
            yield return FriendlyTypeNameFormatter.Instance;
        }
    }
}
