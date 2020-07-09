// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property calculator that accepts evaluate func in simple and full form.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class PropertyCalculator<T> : IPropertyCalculator<T>
    {
        private readonly Func<IPropertyContainer, T> _calculateSimple;
        private readonly Func<IPropertyContainer, (T Value, ValueSource ValueSource)> _calculate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculateSimple">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, T> calculateSimple)
        {
            _calculateSimple = calculateSimple.AssertArgumentNotNull(nameof(calculateSimple));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculate">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, (T Value, ValueSource ValueSource)> calculate)
        {
            _calculate = calculate.AssertArgumentNotNull(nameof(calculate));
        }

        /// <inheritdoc />
        public (T Value, ValueSource ValueSource) Calculate(IPropertyContainer propertyContainer)
        {
            if (_calculateSimple != null)
            {
                T value = _calculateSimple(propertyContainer);
                return (value, ValueSource.Calculated);
            }

            var calculationResult = _calculate(propertyContainer);
            return calculationResult;
        }
    }
}
