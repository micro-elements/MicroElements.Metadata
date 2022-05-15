// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property value calculator.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyCalculator<T> : IMetadata
    {
        /// <summary>
        /// Calculates value.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="searchOptions">SearchOptions provided by client.</param>
        /// <returns>Value and <see cref="ValueSource"/>.</returns>
        (T Value, ValueSource ValueSource) Calculate(IPropertyContainer propertyContainer, SearchOptions searchOptions);
    }
}
