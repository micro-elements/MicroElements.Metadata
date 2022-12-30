// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property value calculator.
    /// </summary>
    public interface IPropertyCalculator : IMetadata
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

    public interface IPropertyCalculatorCovariant<out T> : IPropertyCalculator
    {
        /// <summary>
        /// Calculates value.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="searchOptions">SearchOptions provided by client.</param>
        /// <returns>Value and <see cref="ValueSource"/>.</returns>
        T Calculate(IPropertyContainer propertyContainer, in SearchOptions searchOptions, out ValueSource valueSource);
    }

    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Gets IPropertyCalculator{T} for property.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IPropertyCalculator{T}"/>.</returns>
        public static IPropertyCalculator<T>? GetCalculator<T>(this IProperty<T> property)
        {
            return property.GetComponent<IPropertyCalculator<T>>();
        }
    }
}
