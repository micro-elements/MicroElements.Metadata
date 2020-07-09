namespace MicroElements.Metadata
{
    /// <summary>
    /// Property value calculator.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyCalculator<T>
    {
        /// <summary>
        /// Calculates value.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Value and <see cref="ValueSource"/>.</returns>
        (T Value, ValueSource ValueSource) Calculate(IPropertyContainer propertyContainer);
    }
}
