// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public interface IPropertyCalculator<out T> : IPropertyCalculator
    {
        /// <summary>
        /// Calculates property value.
        /// </summary>
        /// <param name="context">Calculation context.</param>
        /// <returns>Calculated value.</returns>
        T? Calculate(ref CalculationContext context);
    }

    /// <summary>
    /// Property value calculation context.
    /// </summary>
    public ref struct CalculationContext
    {
        /// <summary>
        /// Gets property container.
        /// </summary>
        public IPropertyContainer PropertyContainer { get; }

        /// <summary>
        /// Gets search options.
        /// </summary>
        public SearchOptions SearchOptions { get; }

        /// <summary>
        /// Gets or sets result value source.
        /// </summary>
        public ValueSource? ValueSource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationContext"/> struct.
        /// </summary>
        /// <param name="propertyContainer">Property container for value calculation.</param>
        /// <param name="searchOptions">Search options for property searching.</param>
        public CalculationContext(IPropertyContainer propertyContainer, SearchOptions searchOptions)
            : this()
        {
            PropertyContainer = propertyContainer;
            SearchOptions = searchOptions;
            ValueSource = ValueSource.NotDefined;
        }
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
