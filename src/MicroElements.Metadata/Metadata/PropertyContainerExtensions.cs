// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions methods for <see cref="IPropertyContainer"/>.
    /// </summary>
    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Converts to <see cref="IMutablePropertyContainer"/> if needed.
        /// Returns the same container if it can be casted to <see cref="IMutablePropertyContainer"/>
        /// Or returns new <see cref="MutablePropertyContainer"/> copy of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns><see cref="IMutablePropertyContainer"/>.</returns>
        public static IMutablePropertyContainer ToMutable(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            if (propertyContainer is IMutablePropertyContainer mutablePropertyContainer)
                return mutablePropertyContainer;
            return new MutablePropertyContainer(
                sourceValues: propertyContainer,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

        /// <summary>
        /// Converts to read only <see cref="IPropertyContainer"/> if needed.
        /// Returns read only copy of <paramref name="propertyContainer"/> if it is <see cref="IMutablePropertyContainer"/>.
        /// Or returns the same container.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns><see cref="IPropertyContainer"/>.</returns>
        public static IPropertyContainer ToReadOnly(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            if (propertyContainer is IMutablePropertyContainer)
            {
                return new PropertyContainer(
                    sourceValues: propertyContainer,
                    parentPropertySource: propertyContainer.ParentSource,
                    searchOptions: propertyContainer.SearchOptions);
            }

            return propertyContainer;
        }
    }
}
