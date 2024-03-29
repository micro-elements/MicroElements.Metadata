﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// <see cref="float"/> formatter.
    /// </summary>
    public class FloatFormatter : IValueFormatter<float>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<float> Instance { get; } = new FloatFormatter();

        /// <inheritdoc />
        public string Format(float value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    /// <summary>
    /// <see cref="double"/> formatter.
    /// </summary>
    public class DoubleFormatter : IValueFormatter<double>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<double> Instance { get; } = new DoubleFormatter();

        /// <inheritdoc />
        public string Format(double value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    /// <summary>
    /// <see cref="decimal"/> formatter.
    /// </summary>
    public class DecimalFormatter : IValueFormatter<decimal>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<decimal> Instance { get; } = new DecimalFormatter();

        /// <inheritdoc />
        public string Format(decimal value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }
}
