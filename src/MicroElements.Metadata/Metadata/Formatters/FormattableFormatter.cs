// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using MicroElements.Functional;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Uses <see cref="IFormattable.ToString(string,IFormatProvider)"/> with provided format and optional <see cref="IFormatProvider"/>.
    /// </summary>
    public class FormattableFormatter : IValueFormatter
    {
        private readonly string _typeMatch;
        private readonly Func<Type, bool> _canFormat;
        private readonly string _format;
        private readonly IFormatProvider? _formatProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattableFormatter"/> class.
        /// </summary>
        /// <param name="canFormat">Function that determines whether this formatter can format the specified object type.</param>
        /// <param name="format">Format.</param>
        /// <param name="formatProvider">Optional format provider.</param>
        public FormattableFormatter(
            Expression<Func<Type, bool>> canFormat,
            string format,
            IFormatProvider? formatProvider = null)
        {
            canFormat.AssertArgumentNotNull(nameof(canFormat));
            format.AssertArgumentNotNull(nameof(format));

            _typeMatch = canFormat.Body.ToString();
            _canFormat = canFormat.Compile();
            _format = format;
            _formatProvider = formatProvider;
        }

        public FormattableFormatter(
            string fullTypeName,
            string format,
            IFormatProvider? formatProvider = null)
        {
            fullTypeName.AssertArgumentNotNull(nameof(fullTypeName));
            format.AssertArgumentNotNull(nameof(format));

            _typeMatch = $"type == {fullTypeName}";
            _canFormat = type => type.FullName == fullTypeName;
            _format = format;
            _formatProvider = formatProvider;
        }

        public FormattableFormatter(
            Type type,
            string format,
            IFormatProvider? formatProvider = null)
        {
            type.AssertArgumentNotNull(nameof(type));
            format.AssertArgumentNotNull(nameof(format));

            _typeMatch = $"type == {type.FullName}";
            _canFormat = t => t == type;
            _format = format;
            _formatProvider = formatProvider;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType) => _canFormat(valueType);

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            if (value is IFormattable formattable)
            {
                return formattable.ToString(_format, _formatProvider);
            }

            return null;
        }

        /// <inheritdoc />
        public override string ToString() => $"CanFormat: '{_typeMatch}', Format: '{_format}'";
    }
}
