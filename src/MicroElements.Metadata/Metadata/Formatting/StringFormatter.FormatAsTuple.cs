using System;
using System.Collections;

namespace MicroElements.Metadata.Formatting
{
    public static partial class StringFormatter
    {
        public static string FormatAsTuple(
            this IEnumerable? values,
            string separator = ", ",
            string nullPlaceholder = "null",
            string startSymbol = "(",
            string endSymbol = ")",
            Func<object, string?>? formatValue = null,
            int? maxItems = null,
            int maxTextLength = 4000,
            string trimmedPlaceholder = "...")
        {
            formatValue ??= value => value.FormatValue(nullPlaceholder: nullPlaceholder);

            return MicroElements.Text.StringFormatter.StringFormatter.
                FormatAsTuple(
                    values: values,
                    separator: separator,
                    nullPlaceholder: nullPlaceholder,
                    startSymbol: startSymbol,
                    endSymbol: endSymbol,
                    formatValue: formatValue,
                    maxItems: maxItems,
                    maxTextLength: maxTextLength,
                    trimmedPlaceholder: trimmedPlaceholder);
        }
    }
}
