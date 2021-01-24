// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace MicroElements.Metadata.Formatters
{
    public class FloatFormatter : IValueFormatter<float>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<float> Instance { get; } = new FloatFormatter();

        /// <inheritdoc />
        public string Format(float value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    public class DoubleFormatter : IValueFormatter<double>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<double> Instance { get; } = new DoubleFormatter();

        /// <inheritdoc />
        public string Format(double value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

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
