﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property calculator that accepts evaluate func in simple and full form.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public sealed class PropertyCalculator<T> : IPropertyCalculator<T>
    {
        private readonly Func<IPropertyContainer, SearchOptions, T>? _calculateSimple;
        private readonly Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)>? _calculate;

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
            calculateSimple.AssertArgumentNotNull(nameof(calculateSimple));

            _calculateSimple = calculateSimple;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCalculator{T}"/> class.
        /// </summary>
        /// <param name="calculate">Calculate func.</param>
        public PropertyCalculator(Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)> calculate)
        {
            calculate.AssertArgumentNotNull(nameof(calculate));

            _calculate = calculate;
        }

        /// <inheritdoc />
        public (T Value, ValueSource ValueSource) Calculate(IPropertyContainer propertyContainer, in SearchOptions searchOptions)
        {
            if (_calculateSimple != null)
            {
                T value = _calculateSimple(propertyContainer, searchOptions);
                return (value, ValueSource.Calculated);
            }

            var calculationResult = _calculate!.Invoke(propertyContainer, searchOptions);
            return calculationResult;
        }
    }
}
