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
    public interface INumericInterval : ISchemaComponent
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

        public static string ToIntervalString(this INumericInterval interval, string? intervalDelimeter = ", ")
        {
            intervalDelimeter ??= ", ";
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
    /// Metadata merge mode.
    /// </summary>
    public enum MetadataMergeMode
    {
        /// <summary>
        /// Replace metadata.
        /// </summary>
        Set,

        /// <summary>
        /// Merge metadata. New metadata values appends on Current.
        /// </summary>
        Merge,

        ///// <summary>
        ///// Merge in reverse order. Current metadata appends on New.
        ///// </summary>
        //ReverseMerge,
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="INumericInterval"/> metadata to numeric property or schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source property.</param>
        /// <param name="numericInterval">Numeric interval for property values.</param>
        /// <param name="mergeMode">Merge mode.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetNumericInterval<TSchema>(this TSchema schema, INumericInterval numericInterval, MetadataMergeMode mergeMode = MetadataMergeMode.Set)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            INumericInterval interval = numericInterval;
            if (mergeMode == MetadataMergeMode.Merge && schema.GetNumericInterval() is { } existing)
            {
                interval = new NumericInterval(
                    minimum: numericInterval.Minimum ?? existing.Minimum,
                    exclusiveMinimum: numericInterval.ExclusiveMinimum ?? existing.ExclusiveMinimum,
                    maximum: numericInterval.Maximum ?? existing.Maximum,
                    exclusiveMaximum: numericInterval.ExclusiveMaximum ?? existing.ExclusiveMaximum);
            }

            return schema.SetMetadata(interval);
        }

        public static TSchema MergeNumericInterval<TSchema>(this TSchema schema, INumericInterval numericInterval)
            where TSchema : ISchema
        {
            return SetNumericInterval(schema, numericInterval, MetadataMergeMode.Merge);
        }

        public static INumericInterval? GetNumericInterval(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetSchemaMetadata<INumericInterval>();
        }

        public static TSchema SetMinimum<TSchema>(this TSchema schema, decimal minimum, bool exclusiveMinimum = false)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(minimum, exclusiveMinimum: exclusiveMinimum, null, null));
        }

        public static TSchema SetMaximum<TSchema>(this TSchema schema, decimal maximum, bool exclusiveMaximum = false)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(null, null, Convert.ToDecimal(maximum), exclusiveMaximum: exclusiveMaximum));
        }

        public static TSchema GreaterThan<TSchema>(this TSchema schema, decimal minimum)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(Convert.ToDecimal(minimum), exclusiveMinimum: true, null, null));
        }

        public static TSchema GreaterThanOrEqual<TSchema>(this TSchema schema, decimal minimum)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(Convert.ToDecimal(minimum), exclusiveMinimum: false, null, null));
        }

        public static TSchema LessThan<TSchema>(this TSchema schema, decimal maximum)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(null, null, Convert.ToDecimal(maximum), exclusiveMaximum: true));
        }

        public static TSchema LessThanOrEqual<TSchema>(this TSchema schema, decimal maximum)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schema.AssertPropertyIsNumeric();

            return schema.MergeNumericInterval(new NumericInterval(null, null, Convert.ToDecimal(maximum), exclusiveMaximum: false));
        }

        private static void AssertPropertyIsNumeric<TSchema>(this TSchema schema)
            where TSchema : ISchema
        {
            bool isNumericType = schema.Type.IsNumericType() || schema.Type.IsNullableNumericType();
            if (!isNumericType)
                throw new ArgumentException($"{nameof(INumericInterval)} can be set only for numeric properties. Property '{schema.Name}' has type '{schema.Type}'.");
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
