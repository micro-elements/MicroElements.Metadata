// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// String formatting.
    /// </summary>
    public static class Formatter
    {
        /// <summary>
        /// Gets default single item formatter.
        /// </summary>
        public static IValueFormatter SingleValueFormatter { get; } = new CompositeFormatter(DefaultFormatProvider.Instance.GetFormatters());

        /// <summary>
        /// Gets FullToStringFormatter that formats with <see cref="SingleValueFormatter"/> and can format collections.
        /// </summary>
        public static IValueFormatter FullToStringFormatter { get; } = new CompositeFormatter()
            .With(SingleValueFormatter)
            .With(new CollectionFormatter(SingleValueFormatter))
            .With(new KeyValuePairFormatter(SingleValueFormatter))
            .With(new ValueTuplePairFormatter(SingleValueFormatter))
            .With(DefaultToStringFormatter.Instance);

        /// <summary>
        /// Gets Recursive formatter that uses itself for collections and key value pairs and tuples.
        /// </summary>
        public static IValueFormatter FullRecursiveFormatter { get; } = new CompositeFormatter()
            .With(new DefaultFormatProvider().GetFormatters())
            .With(new CollectionFormatter(new RuntimeFormatter(() => FullRecursiveFormatter)))
            .With(new KeyValuePairFormatter(new RuntimeFormatter(() => FullRecursiveFormatter)))
            .With(new ValueTuplePairFormatter(new RuntimeFormatter(() => FullRecursiveFormatter)))
            .With(DefaultToStringFormatter.Instance);
    }
}
