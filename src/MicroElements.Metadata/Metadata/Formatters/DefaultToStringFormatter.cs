// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Formatters
{
    public class DefaultToStringFormatter : IValueFormatter<object>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<object> Instance { get; } = new DefaultToStringFormatter();

        /// <inheritdoc />
        public string? Format(object? value) => value?.ToString();
    }
}
