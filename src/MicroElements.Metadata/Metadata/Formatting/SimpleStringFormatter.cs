// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Returns string as is.
    /// </summary>
    public class SimpleStringFormatter : IValueFormatter<string>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<string> Instance { get; } = new SimpleStringFormatter();

        /// <inheritdoc />
        public string? Format(string? value) => value;
    }
}
