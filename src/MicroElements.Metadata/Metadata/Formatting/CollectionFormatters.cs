using System.Collections.Generic;

namespace MicroElements.Metadata.Formatting
{
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
