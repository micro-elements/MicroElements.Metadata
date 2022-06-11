// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Collections.Extensions.NotNull;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions methods for <see cref="IPropertyContainer"/>.
    /// </summary>
    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Clones source <paramref name="propertyContainer"/> as <see cref="MutablePropertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Mutable copy of initial property container.</returns>
        public static IMutablePropertyContainer CloneAsMutable(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return new MutablePropertyContainer(
                sourceValues: propertyContainer.Properties,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

        /// <summary>
        /// Clones source <paramref name="propertyContainer"/> as <see cref="PropertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>ReadOnly copy of initial property container.</returns>
        public static IPropertyContainer CloneAsReadOnly(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return new PropertyContainer(
                sourceValues: propertyContainer.Properties,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

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

        /// <summary>
        /// Merges <see cref="IPropertyContainer"/> objects to single <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="otherContainer">Container to merge to source container.</param>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <returns>New <see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer Merge(
            this IPropertyContainer propertyContainer,
            IPropertyContainer otherContainer,
            PropertyAddMode mergeMode = PropertyAddMode.Set)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            otherContainer.AssertArgumentNotNull(nameof(otherContainer));

            return propertyContainer.ToMutable().WithValues(otherContainer, mergeMode);
        }

        /// <summary>
        /// Merges <see cref="IPropertyContainer"/> objects to single <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="containers">Source objects.</param>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <returns>New <see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer Merge(
            this IEnumerable<IPropertyContainer?>? containers,
            PropertyAddMode mergeMode = PropertyAddMode.Set)
        {
            if (containers is null)
                return PropertyContainer.Empty;
            if (containers is ICollection<IPropertyContainer?> { Count: 1 } collection)
                return collection.First() ?? PropertyContainer.Empty;
            if (containers is IReadOnlyCollection<IPropertyContainer?> { Count: 1 } readOnly)
                return readOnly.First() ?? PropertyContainer.Empty;

            MutablePropertyContainer? merger = null;

            foreach (var container in containers)
            {
                if (container != null)
                {
                    if (merger == null)
                    {
                        merger = new MutablePropertyContainer(
                            sourceValues: container,
                            searchOptions: container.SearchOptions);
                        continue;
                    }

                    merger.WithValues(container, mergeMode);
                }
            }

            return merger ?? PropertyContainer.Empty;
        }

        /// <summary>
        /// Merges composite properties to one container.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <param name="properties">Properties to merge.</param>
        /// <returns>New <see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer MergeProperties(
            this IPropertyContainer propertyContainer,
            PropertyAddMode mergeMode = PropertyAddMode.Set,
            params IProperty<IPropertyContainer>[]? properties)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return properties
                .NotNull()
                .Select(property => propertyContainer.GetValue(property))
                .Merge(mergeMode);
        }

        /// <summary>
        /// Gets list items for property that implemented as <see cref="IPropertyContainer"/> list.
        /// Example:
        /// <code>
        /// - Source: IPropertyContainer
        ///   - ListProperty: IPropertyContainer
        ///     - ListItem: IPropertyContainer
        ///     - ListItem: IPropertyContainer
        /// </code>
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="listProperty">List property.</param>
        /// <returns>ListItem enumeration.</returns>
        public static IEnumerable<IPropertyContainer> GetListItems(
            this IPropertyContainer propertyContainer,
            IProperty<IPropertyContainer> listProperty)
        {
            IPropertyContainer list = propertyContainer.GetValue(listProperty) ?? PropertyContainer.Empty;
            return list
                .Select(pv => pv.ValueUntyped as IPropertyContainer)
                .Where(container => container != null)!;
        }

        /// <summary>
        /// Gets list items for property <paramref name="listProperty"/>.
        /// Then enriches each list item with common values from <paramref name="commonValues"/>.
        /// </summary>
        /// <param name="containers">Source container.</param>
        /// <param name="listProperty">List property.</param>
        /// <param name="commonValues">Property that contains common values for each list item.</param>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <returns>List items enriched with values from <paramref name="commonValues"/>.</returns>
        public static IEnumerable<IPropertyContainer> GetListItemsEnriched(
            this IEnumerable<IPropertyContainer> containers,
            IProperty<IPropertyContainer> listProperty,
            IProperty<IPropertyContainer> commonValues,
            PropertyAddMode mergeMode = PropertyAddMode.Set)
        {
            var joinedItems = containers
                .Select(container => new
                {
                    List = container.GetListItems(listProperty),
                    Common = container.GetValue(commonValues),
                })
                .SelectMany(a => a.List.Select(listItem => PropertyContainer.Merge(mergeMode, a.Common, listItem)));

            return joinedItems;
        }

        /// <summary>
        /// Gets list items for property <paramref name="listProperty"/>.
        /// Then enriches each list item with common values from <paramref name="commonValues1"/> and <paramref name="commonValues2"/>.
        /// </summary>
        /// <param name="containers">Source container.</param>
        /// <param name="listProperty">List property.</param>
        /// <param name="commonValues1">First property that contains common values for each list item.</param>
        /// <param name="commonValues2">Second property that contains common values for each list item.</param>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <returns>List items enriched with values from <paramref name="commonValues1"/> and <paramref name="commonValues2"/>.</returns>
        public static IEnumerable<IPropertyContainer> GetListItemsEnriched(
            this IEnumerable<IPropertyContainer> containers,
            IProperty<IPropertyContainer> listProperty,
            IProperty<IPropertyContainer> commonValues1,
            IProperty<IPropertyContainer> commonValues2,
            PropertyAddMode mergeMode = PropertyAddMode.Set)
        {
            var joinedItems = containers
                .Select(container => new
                {
                    List = container.GetListItems(listProperty),
                    Common1 = container.GetValue(commonValues1),
                    Common2 = container.GetValue(commonValues2),
                })
                .SelectMany(a => a.List.Select(listItem => PropertyContainer.Merge(mergeMode, a.Common1, a.Common2, listItem)));

            return joinedItems;
        }

        /// <summary>
        /// Filters source property container by provided properties and comparer.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="properties">Properties that should be copied to new container.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <returns>New property container.</returns>
        public static IPropertyContainer FilterByProperties(
            this IPropertyContainer propertyContainer,
            IEnumerable<IProperty> properties,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            propertyComparer ??= PropertyComparer.DefaultEqualityComparer;
            var propertySet = properties.ToHashSet(propertyComparer);

            var propertyValues = propertyContainer
                .Properties
                .Where(pv => propertySet.Contains(pv.PropertyUntyped));

            return new PropertyContainer(sourceValues: propertyValues, searchOptions: propertyContainer.SearchOptions);
        }

        public static IPropertyContainer ToPropertyContainer(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            return new PropertyContainer(propertyValues, parentPropertySource, searchOptions);
        }
    }
}
