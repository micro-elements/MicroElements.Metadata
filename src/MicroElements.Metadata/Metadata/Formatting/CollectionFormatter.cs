// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using MicroElements.CodeContracts;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Reflection.TypeExtensions;
using MicroElements.Text.StringFormatter;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatter for collection types.
    /// </summary>
    public class CollectionFormatter :
        IValueFormatter,
        ICompositeBuilder<CollectionFormatter, IConfigure<CollectionFormatterSettings>>
    {
        private readonly CollectionFormatterSettings _formatterSetting;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionFormatter" /> class.
        /// </summary>
        /// <param name="formatterSetting">Formatting settings.</param>
        public CollectionFormatter(CollectionFormatterSettings? formatterSetting = null)
        {
            _formatterSetting = formatterSetting?.Clone() ?? new CollectionFormatterSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionFormatter" /> class.
        /// </summary>
        /// <param name="valueFormatter">Collection values formatter.</param>
        public CollectionFormatter(IValueFormatter valueFormatter)
        {
            valueFormatter.AssertArgumentNotNull(nameof(valueFormatter));
            _formatterSetting = new CollectionFormatterSettings { ValueFormatter = valueFormatter };
        }

        /// <inheritdoc />
        public CollectionFormatter With(IConfigure<CollectionFormatterSettings> configure)
        {
            var settings = _formatterSetting.Clone();
            configure.Configure(settings);
            return new CollectionFormatter(settings);
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType) => valueType.IsAssignableTo(typeof(ICollection)) || valueType.Name.StartsWith("IReadOnlyCollection");

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            if (value is ICollection values)
            {
                return values.FormatAsTuple(
                    startSymbol: _formatterSetting.StartSymbol,
                    endSymbol: _formatterSetting.EndSymbol,
                    separator: _formatterSetting.Separator,
                    nullPlaceholder: _formatterSetting.NullPlaceholder,
                    maxItems: _formatterSetting.MaxItems,
                    maxTextLength: _formatterSetting.MaxTextLength,
                    trimmedPlaceholder: _formatterSetting.TrimmedPlaceholder,
                    formatValue: item => FormatCollectionItem(item));
            }

            return null;
        }

        private string FormatCollectionItem(object item)
        {
            return _formatterSetting.ValueFormatter.Format(item, typeof(object)) ?? _formatterSetting.NullPlaceholder;
        }
    }

    /// <summary>
    /// Format settings to be used in collection formatting.
    /// </summary>
    public class CollectionFormatterSettings : ICloneable<CollectionFormatterSettings>
    {
        /// <summary> The value that uses to separate items. DefaultValue = ', '. </summary>
        public string Separator { get; set; } = ", ";

        /// <summary> The value that renders if item is <see langword="null"/>. DefaultValue = `"null"`. </summary>
        public string NullPlaceholder { get; set; } = "null";

        /// <summary> Start symbol. DefaultValue = '['. </summary>
        public string StartSymbol { get; set; } = "[";

        /// <summary> End symbol. DefaultValue = ']'. </summary>
        public string EndSymbol { get; set; } = "]";

        /// <summary> The max number of items that will be formatted. By default not limited. </summary>
        public int? MaxItems { get; set; } = null;

        /// <summary> Max result text length. Used to limit result text size. DefaultValue=`4000`. </summary>
        public int MaxTextLength { get; set; } = 4000;

        /// <summary> The value that replaces trimmed part of sequence. DefaultValue = `"..."`. </summary>
        public string TrimmedPlaceholder { get; set; } = "...";

        /// <summary> Formatter for every single item. </summary>
        public IValueFormatter ValueFormatter { get; set; } = null!;
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
        public static FormatterBuilder ConfigureCollectionFormatter(this FormatterBuilder builder, Action<CollectionFormatterSettings> configure)
        {
            return builder.ConfigureFormatter<CollectionFormatterSettings>(configure);
        }
    }
}
