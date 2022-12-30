// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatter for <see cref="KeyValuePair{TKey,TValue}"/>.
    /// </summary>
    public class KeyValuePairFormatter :
        IValueFormatter<KeyValuePair<string, object?>>,
        IConfigurableBuilder<KeyValuePairFormatter, KeyValuePairFormatterSettings>
    {
        private readonly KeyValuePairFormatterSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairFormatter"/> class.
        /// </summary>
        /// <param name="valueFormatter">Value formatter.</param>
        /// <param name="format">Optional format in terms of <see cref="string.Format(IFormatProvider,string,object)"/>.</param>
        public KeyValuePairFormatter(IValueFormatter valueFormatter, string format = "({0}: {1})")
        {
            _settings = new KeyValuePairFormatterSettings { ValueFormatter = valueFormatter, Format = format };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairFormatter"/> class.
        /// </summary>
        /// <param name="settings">Setting to be used for formatting.</param>
        public KeyValuePairFormatter(KeyValuePairFormatterSettings? settings = null)
        {
            _settings = settings?.Clone() ?? new KeyValuePairFormatterSettings();
        }

        /// <inheritdoc />
        public string Format(KeyValuePair<string, object?> keyValuePair)
        {
            return string.Format(_settings.Format, keyValuePair.Key, _settings.ValueFormatter.TryFormat(keyValuePair.Value));
        }

        /// <inheritdoc />
        public KeyValuePairFormatterSettings GetState() => _settings.Clone();

        /// <inheritdoc />
        public KeyValuePairFormatter With(KeyValuePairFormatterSettings settings) => new KeyValuePairFormatter(settings);
    }

    /// <summary>
    /// Settings for <see cref="KeyValuePairFormatter"/>.
    /// </summary>
    public class KeyValuePairFormatterSettings : ICloneable<KeyValuePairFormatterSettings>
    {
        /// <summary>
        /// Gets or sets formatter that will be used for value formatting.
        /// </summary>
        public IValueFormatter ValueFormatter { get; set; } = DefaultToStringFormatter.Instance;

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
