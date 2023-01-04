// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Calculate property value delegate.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public delegate T? CalculateDelegate<out T>(ref CalculationContext context);

    /// <summary>
    /// Property calculator that accepts evaluate func in simple and full form.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public sealed class PropertyCalculator<T> : IPropertyCalculator<T>
    {
        private readonly Func<IPropertyContainer, SearchOptions, T>? _calculateSimple;
        private readonly Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)>? _calculate;
        private readonly CalculateDelegate<T>? _calculateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculate">Calculate delegate.</param>
        public PropertyCalculator(CalculateDelegate<T>? calculate)
        {
            _calculateDelegate = calculate.AssertArgumentNotNull(nameof(calculate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculateSimple">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, T> calculateSimple)
        {
            calculateSimple.AssertArgumentNotNull(nameof(calculateSimple));

            _calculateSimple = (container, options) => calculateSimple(container);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculateSimple">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, SearchOptions, T> calculateSimple)
        {
            _calculateSimple = calculateSimple.AssertArgumentNotNull(nameof(calculateSimple));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculate">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)> calculate)
        {
            _calculate = calculate.AssertArgumentNotNull(nameof(calculate));
        }

        /// <inheritdoc />
        public T? Calculate(ref CalculationContext context)
        {
            if (_calculateSimple is { } calculateSimple)
            {
                T value = calculateSimple(context.PropertyContainer, context.SearchOptions);
                context.ValueSource = ValueSource.Calculated;
                return value;
            }

            if (_calculateDelegate is { } calculateDelegate)
            {
                T? value = calculateDelegate(ref context);
                context.ValueSource = context.ValueSource;
                return value;
            }

            if (_calculate is { } calculate)
            {
                var calculationResult = calculate(context.PropertyContainer, context.SearchOptions);
                context.ValueSource = calculationResult.ValueSource;
                return calculationResult.Value;
            }

            throw new InvalidOperationException("Invalid property calculator, no calculation delegate provided.");
        }
    }
}
