// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using MicroElements.Functional;
using MicroElements.Validation;
using MicroElements.Validation.Rules;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents numeric interval metadata.
    /// Describes valid numeric interval for property value.
    /// Aligned with JSON Schema definition: https://tools.ietf.org/html/draft-fge-json-schema-validation-00.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface INumericInterval : IMetadata
    {
        /// <summary>
        /// Minimum value allowed for the property value.
        /// <seealso cref="ExclusiveMinimum"/>.
        /// </summary>
        decimal? Minimum { get; }

        /// <summary>
        /// <para>If <see cref="ExclusiveMinimum"/> is not present, or has boolean value false, then the instance is valid if it is greater than, or equal to, the value of <see cref="Minimum"/>.</para>
        /// <para>If <see cref="ExclusiveMinimum"/> is present and has boolean value true, the instance is valid if it is strictly greater than the value of <see cref="Minimum"/>.</para>
        /// </summary>
        bool? ExclusiveMinimum { get; }

        /// <summary>
        /// Maximum value allowed for the property value.
        /// <seealso cref="ExclusiveMaximum"/>.
        /// </summary>
        decimal? Maximum { get; }

        /// <summary>
        /// <para>If <see cref="ExclusiveMaximum"/> is not present, or has boolean value false, then the instance is valid if it is lower than, or equal to, the value of <see cref="Maximum"/>.</para>
        /// <para>If <see cref="ExclusiveMaximum"/> has boolean value true, the instance is valid if it is strictly lower than the value of <see cref="Maximum"/>.</para>
        /// </summary>
        bool? ExclusiveMaximum { get; }
    }

    public static class NumericIntervalExtensions
    {
        public static bool IsLeftIncluded(this INumericInterval interval)
        {
            return !interval.ExclusiveMinimum.HasValue || interval.ExclusiveMinimum == false;
        }

        public static bool IsRightIncluded(this INumericInterval interval)
        {
            return !interval.ExclusiveMaximum.HasValue || interval.ExclusiveMaximum == false;
        }

        public static string ToIntervalString(this INumericInterval interval, string? intervalDelimeter = "..")
        {
            intervalDelimeter ??= "..";
            string left = interval.Minimum.HasValue ? interval.Minimum.Value.ToString(NumberFormatInfo.InvariantInfo) : "-Infinity";
            string right = interval.Maximum.HasValue ? interval.Maximum.Value.ToString(NumberFormatInfo.InvariantInfo) : "+Infinity";
            string leftBracket = interval.IsLeftIncluded() ? "[" : "(";
            string rightBracket = interval.IsRightIncluded() ? "]" : ")";

            return $"{leftBracket}{left}{intervalDelimeter}{right}{rightBracket}";
        }
    }

    /// <inheritdoc cref="INumericInterval"/>.
    public sealed class NumericInterval : INumericInterval
    {
        /// <inheritdoc />
        public decimal? Minimum { get; }

        /// <inheritdoc />
        public bool? ExclusiveMinimum { get; }

        /// <inheritdoc />
        public decimal? Maximum { get; }

        /// <inheritdoc />
        public bool? ExclusiveMaximum { get; }

        public NumericInterval(
            decimal? minimum,
            bool? exclusiveMinimum,
            decimal? maximum,
            bool? exclusiveMaximum)
        {
            Minimum = minimum;
            ExclusiveMinimum = exclusiveMinimum;
            Maximum = maximum;
            ExclusiveMaximum = exclusiveMaximum;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="INumericInterval"/> metadata to numeric property.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="numericInterval">Numeric interval for property values.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetNumericInterval<T>(this IProperty<T> property, INumericInterval numericInterval)
        {
            property.AssertArgumentNotNull(nameof(property));
            property.AssertPropertyIsNumeric();

            return property.SetMetadata(numericInterval);
        }

        public static INumericInterval? GetNumericInterval(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<INumericInterval>();
        }

        public static IProperty<T> GreaterThan<T>(this IProperty<T> property, T minimum)
        {
            property.AssertArgumentNotNull(nameof(property));
            property.AssertPropertyIsNumeric();

            return property.SetNumericInterval(new NumericInterval(Convert.ToDecimal(minimum), exclusiveMinimum: true, null, null));
        }

        public static IProperty<T> GreaterThanOrEqual<T>(this IProperty<T> property, T minimum)
        {
            property.AssertArgumentNotNull(nameof(property));
            property.AssertPropertyIsNumeric();

            return property.SetNumericInterval(new NumericInterval(Convert.ToDecimal(minimum), exclusiveMinimum: false, null, null));
        }

        public static IProperty<T> LessThan<T>(this IProperty<T> property, T maximum)
        {
            property.AssertArgumentNotNull(nameof(property));
            property.AssertPropertyIsNumeric();

            return property.SetNumericInterval(new NumericInterval(null, null, Convert.ToDecimal(maximum), exclusiveMaximum: true));
        }

        public static IProperty<T> LessThanOrEqual<T>(this IProperty<T> property, T maximum)
        {
            property.AssertArgumentNotNull(nameof(property));
            property.AssertPropertyIsNumeric();

            return property.SetNumericInterval(new NumericInterval(null, null, Convert.ToDecimal(maximum), exclusiveMaximum: false));
        }

        private static void AssertPropertyIsNumeric<T>(this IProperty<T> property)
        {
            bool isNumericType = typeof(T).IsNumericType() || typeof(T).IsNullableNumericType();
            if (!isNumericType)
                throw new ArgumentException($"{nameof(INumericInterval)} can be set only for numeric properties. Property '{property.Name}' has type '{property.Type}'.");
        }
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class ShouldBeInInterval<T> : PropertyValidationRule<T>
    {
        private readonly INumericInterval? _numericInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldBeInInterval{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="numericInterval">Optional <see cref="INumericInterval"/>.</param>
        public ShouldBeInInterval(IProperty<T> property, INumericInterval? numericInterval = null)
            : base(property, "{propertyName} should be in range {range}.")
        {
            _numericInterval = numericInterval ?? Property.GetNumericInterval();
            this.ConfigureMessage(message => message.WithProperty("range", _numericInterval.ToIntervalString()));
        }

        /// <inheritdoc />
        protected override bool IsValid(T? value)
        {
            if (_numericInterval != null)
            {
                decimal numericValue = Convert.ToDecimal(value);
                bool valid = true;

                if (_numericInterval.Minimum.HasValue)
                {
                    if (_numericInterval.IsLeftIncluded())
                        valid = numericValue >= _numericInterval.Minimum;
                    else
                        valid = numericValue > _numericInterval.Minimum;
                }

                if (!valid)
                    return false;

                if (_numericInterval.Maximum.HasValue)
                {
                    if (_numericInterval.IsRightIncluded())
                        valid = numericValue <= _numericInterval.Maximum;
                    else
                        valid = numericValue < _numericInterval.Maximum;
                }

                if (!valid)
                    return false;
            }

            return true;
        }
    }
}
