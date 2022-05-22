// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Default format provider.
    /// Provides:
    /// - string formatting
    /// - floating numbers formatting (float, double, decimal)
    /// - 
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

            // Floating numbers
            yield return FloatFormatter.Instance;
            yield return DoubleFormatter.Instance;
            yield return DecimalFormatter.Instance;

            // Date and Time
            yield return DateTimeFormatter.IsoShort;
            yield return DateTimeOffsetFormatter.Iso;

            // NodaTime Formatters
            foreach (var nodaTimeFormatter in new NodaTimeIsoFormatters().GetFormatters())
            {
                yield return nodaTimeFormatter;
            }
        }
    }

    public class CollectionFormatters : IValueFormatterProvider
    {
        private readonly IValueFormatter _valueFormatter;

        public CollectionFormatters(IValueFormatter valueFormatter)
        {
            _valueFormatter = valueFormatter;
        }

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            yield return new CollectionFormatter(_valueFormatter);
            yield return new KeyValuePairFormatter(_valueFormatter);
            yield return new ValueTuplePairFormatter(_valueFormatter);
        }
    }
}
