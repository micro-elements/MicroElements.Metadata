// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Checks that property value exists.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Required<T> : NotNull<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Required{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        public Required(IProperty<T> property)
            : base(property)
        {
        }
    }

    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static partial class ValidationRule
    {
        /// <summary>
        /// Checks that property value exists.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <returns><see cref="Required{T}"/> validation rule.</returns>
        public static Required<T> Required<T>(this IProperty<T> property)
        {
            return new Required<T>(property);
        }
    }
}
