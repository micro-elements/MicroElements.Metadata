// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// <see cref="Nullable{T}"/> type formatter.
    /// Adapts base type formatter.
    /// </summary>
    public class NullableFormatter : IValueFormatter
    {
        private readonly IValueFormatter _baseFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableFormatter"/> class.
        /// </summary>
        /// <param name="baseFormatter">Nullable underlying type formatter.</param>
        public NullableFormatter(IValueFormatter baseFormatter)
        {
            _baseFormatter = baseFormatter.AssertArgumentNotNull(nameof(baseFormatter));
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType) => true;

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            if (value == null)
                return null;

            return _baseFormatter.Format(value, valueType);
        }
    }
}
