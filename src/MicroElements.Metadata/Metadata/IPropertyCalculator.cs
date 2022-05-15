// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property value calculator.
    /// </summary>
    public interface IPropertyCalculator
    {
    }

    /// <summary>
    /// Property value calculator.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyCalculator<T> : IPropertyCalculator
    {
        /// <summary>
        /// Calculates value.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="searchOptions">SearchOptions provided by client.</param>
        /// <returns>Value and <see cref="ValueSource"/>.</returns>
        (T Value, ValueSource ValueSource) Calculate(IPropertyContainer propertyContainer, in SearchOptions searchOptions);
    }

    /// <summary>
    /// Represents property that has calculator.
    /// </summary>
    public interface IHasCalculator
    {
        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        IPropertyCalculator? Calculator { get; }
    }

    /// <summary>
    /// Represents property that has calculator.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IHasCalculator<T> : IHasCalculator
    {
        /// <inheritdoc />
        IPropertyCalculator? IHasCalculator.Calculator => Calculator;

        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        new IPropertyCalculator<T>? Calculator { get; }
    }

    /// <summary>
    /// PropertyCalculator extensions.
    /// </summary>
    public static class HasCalculatorExtensions
    {
        /// <summary>
        /// Gets IPropertyCalculator for property.
        /// Looks whether the property is <see cref="IHasCalculator{T}"/> then searches <see cref="IPropertyCalculator{T}"/> component.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IHasCalculator{T}"/>.</returns>
        public static IPropertyCalculator<T>? GetCalculator<T>(this IProperty<T> property)
        {
            if (property is IHasCalculator<T> hasCalculator)
                return hasCalculator.Calculator;

            var propertyCalculator = property.GetComponent<IPropertyCalculator<T>>();
            return propertyCalculator;
        }
    }
}
