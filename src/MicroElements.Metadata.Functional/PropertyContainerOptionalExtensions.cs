// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.CodeContracts;
using MicroElements.Functional;

namespace MicroElements.Metadata.Functional
{
    public static class PropertyContainerOptionalExtensions
    {
        /// <summary>
        /// Sets optional value if <paramref name="value"/> is in Some state.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, in Option<T> value)
        {
            Assertions.AssertArgumentNotNull(propertyContainer, nameof(propertyContainer));

            value.Match(val => propertyContainer.SetValue(property, val), () => { });
        }

        /// <summary>
        /// Sets optional value if <paramref name="value"/> is in Some state.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T?> property, in Option<T> value)
            where T : struct
        {
            Assertions.AssertArgumentNotNull(propertyContainer, nameof(propertyContainer));

            value.Match(val => propertyContainer.SetValue(property, val), () => { });
        }
    }
}
