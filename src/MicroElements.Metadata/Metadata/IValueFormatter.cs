// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents value formatter.
    /// Formats value to string.
    /// </summary>
    public interface IValueFormatter
    {
        /// <summary>
        /// Gets the type that formatter can format.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Formats <paramref name="value"/> to string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Formatted string.</returns>
        string? FormatUntyped(object? value);
    }
}
