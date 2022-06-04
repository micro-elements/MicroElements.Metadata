// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.CodeContracts;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Formatter for <see cref="KeyValuePair{TKey,TValue}"/>.
    /// </summary>
    public class KeyValuePairFormatter :
        IValueFormatter<KeyValuePair<string, object?>>,
        ICompositeBuilder<KeyValuePairFormatter, IConfigure<KeyValuePairFormatterSettings>>
    {
        private readonly IValueFormatter _valueFormatter;
        private readonly string _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairFormatter"/> class.
        /// </summary>
        /// <param name="valueFormatter">Value formatter.</param>
        /// <param name="format">Optional format in terms of <see cref="string.Format(IFormatProvider,string,object)"/>.</param>
        public KeyValuePairFormatter(IValueFormatter valueFormatter, string format = "({0}: {1})")
        {
            _valueFormatter = valueFormatter.AssertArgumentNotNull(nameof(valueFormatter));
            _format = format;
        }

        /// <inheritdoc />
        public string Format(KeyValuePair<string, object?> keyValuePair)
        {
            return string.Format(_format, keyValuePair.Key, _valueFormatter.TryFormat(keyValuePair.Value));
        }

        /// <inheritdoc />
        public KeyValuePairFormatter With(IConfigure<KeyValuePairFormatterSettings> configure)
        {
            KeyValuePairFormatterSettings keyValueFormat = new() { Format = _format };
            configure.Configure(keyValueFormat);
            return new KeyValuePairFormatter(_valueFormatter, keyValueFormat.Format);
        }
    }

    /// <summary>
    /// Settings for <see cref="KeyValuePairFormatter"/>.
    /// </summary>
    public class KeyValuePairFormatterSettings
    {
        /// <summary>
        /// Gets or sets the string format in terms of <see cref="string.Format(IFormatProvider,string,object)"/>.
        /// </summary>
        public string Format { get; set; } = "({0}: {1})";
    }

    /// <summary>
    /// <see cref="FormatterBuilder"/> extensions.
    /// </summary>
    public static partial class FormatterBuilderExtensions
    {
        /// <summary>
        /// Configures <see cref="CollectionFormatter"/>.
        /// Can be called multiple times.
        /// </summary>
        public static FormatterBuilder ConfigureKeyValuePairFormatter(this FormatterBuilder builder, Action<KeyValuePairFormatterSettings> configure)
        {
            return builder.ConfigureFormatter<KeyValuePairFormatterSettings>(configure);
        }
    }
}
