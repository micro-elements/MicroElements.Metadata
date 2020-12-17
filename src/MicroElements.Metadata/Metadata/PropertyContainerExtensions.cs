// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
                sourceValues: propertyContainer.Properties,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

        /// <summary>
        /// Converts to read only <see cref="IPropertyContainer"/> if needed.
        /// Returns read only copy of <paramref name="propertyContainer"/> if it is <see cref="IMutablePropertyContainer"/>.
        /// Or returns the same container.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="flattenHierarchy">Flatten container hierarchy.</param>
        /// <returns><see cref="IPropertyContainer"/>.</returns>
        public static IPropertyContainer ToReadOnly(this IPropertyContainer propertyContainer, bool flattenHierarchy = true)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            if (propertyContainer is IMutablePropertyContainer)
            {
                if (flattenHierarchy && propertyContainer.ParentSource != null && propertyContainer.ParentSource.Count > 0)
                {
                    return propertyContainer
                        .Flatten()
                        .ToReadOnly(flattenHierarchy: false);
                }

                return new PropertyContainer(
                    sourceValues: propertyContainer.Properties,
                    parentPropertySource: propertyContainer.ParentSource,
                    searchOptions: propertyContainer.SearchOptions);
            }

            return propertyContainer;
        }

        /// <summary>
        /// Gets container hierarchy from oldest parent to the current container.
        /// </summary>
        /// <param name="propertyContainer">Source container.</param>
        /// <returns>Container hierarchy.</returns>
        public static IReadOnlyCollection<IPropertyContainer> GetHierarchy(this IPropertyContainer propertyContainer)
        {
            var history = new Stack<IPropertyContainer>();
            history.Push(propertyContainer);

            IPropertyContainer current = propertyContainer;
            while (current.ParentSource != null && current.ParentSource != PropertyContainer.Empty)
            {
                history.Push(current.ParentSource);
                current = current.ParentSource;
            }

            return history;
        }

        /// <summary>
        /// Flattens container hierarchy to single container (from oldest to current).
        /// </summary>
        /// <param name="propertyContainer">Source container.</param>
        /// <returns>New container.</returns>
        public static IPropertyContainer Flatten(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            if (propertyContainer.ParentSource != null && propertyContainer.ParentSource.Count > 0)
            {
                var merger = new MutablePropertyContainer(searchOptions: propertyContainer.SearchOptions);

                var hierarchy = GetHierarchy(propertyContainer);
                foreach (IPropertyContainer ancestor in hierarchy)
                {
                    merger.WithValues(ancestor, PropertyAddMode.Set);
                }

                return merger;
            }

            return propertyContainer;
        }
    }
}
