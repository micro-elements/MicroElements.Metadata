// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Default ToString formatter.
    /// Uses <see cref="object.ToString"/> method.
    /// </summary>
    public class DefaultToStringFormatter : IValueFormatter<object>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<object> Instance { get; } = new DefaultToStringFormatter();

        /// <inheritdoc />
        public string? Format(object? value)
        {
            if (value is null)
                return null;

            /* NOTE: Nullable types boxed as its underlying type. So no need to process nullability case. */
            return value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }
    }
}
