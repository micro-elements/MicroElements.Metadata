// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Default format provider.
    /// </summary>
    public class DefaultFormatProvider : IValueFormatterProvider
    {
        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            // String
            yield return SimpleStringFormatter.Instance;

            // Floating numbers (integers formats by default formatter)
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

            // Type
            yield return FriendlyTypeNameFormatter.Instance;
        }
    }
}
