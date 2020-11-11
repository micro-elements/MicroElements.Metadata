// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Reporting.Styling
{
    /// <summary>
    /// Mark property with <see cref="FormatAttribute"/> to attach <see cref="ICellFormatProvider"/> to it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of <see cref="ICellFormatProvider"/>.
        /// </summary>
        public Type CellFormatProviderType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatAttribute"/> class.
        /// </summary>
        /// <param name="cellFormatProviderType">Type of <see cref="ICellFormatProvider"/>.</param>
        public FormatAttribute(Type cellFormatProviderType) => CellFormatProviderType = cellFormatProviderType;
    }
}
