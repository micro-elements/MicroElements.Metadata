// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions for <see cref="IPropertyContainer"/>.
    /// </summary>
    public static class PropertyContainerExtensions
    {
        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, Search search)
        {
            search.Condition.AssertArgumentNotNull(nameof(search.Condition));

            // Ищем в своих свойствах.
            IPropertyValue propertyValue = propertyContainer.FirstOrDefault(search.Condition);
            if (propertyValue != null)
                return propertyValue;

            // Ищем у родителя
            if (search.SearchInParent && propertyContainer.ParentSource != null)
            {
                propertyValue = propertyContainer.ParentSource.GetPropertyValueUntyped(search);
                if (propertyValue != null)
                    return propertyValue;
            }

            return null;
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(this IPropertyContainer propertyContainer, Search search) =>
            (IPropertyValue<T>)propertyContainer.GetPropertyValueUntyped(search);

        /// <summary>
        /// Gets property and value by property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <param name="calculateValue">Calculate value if value was not found.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            bool searchInParent = true,
            bool calculateValue = true)
        {
            IPropertyValue<T> propertyValue = (IPropertyValue<T>)propertyContainer.GetPropertyValueUntyped(property, searchInParent);

            if (propertyValue.HasValue())
                return propertyValue;

            // Свойство не в списке свойств, но может его можно вычислить.
            if (calculateValue && property.Calculate != null)
            {
                var calculatedValue = property.Calculate(propertyContainer);
                return new PropertyValue<T>(property, calculatedValue, ValueSource.Calculated);
            }

            // Вернем значение по умолчанию.
            return new PropertyValue<T>(property, property.DefaultValue(), ValueSource.DefaultValue);
        }

        /// <summary>
        /// Gets property and value by property.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, IProperty property, bool searchInParent = true) =>
            propertyContainer.GetPropertyValueUntyped(Search.ByProperty(property).SearchInParent(searchInParent));

        /// <summary>
        /// Returns true if <paramref name="propertyContainer"/> has property.
        /// Also searches in parent sources.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue(this IPropertyContainer propertyContainer, IProperty property, bool searchInParent = true) =>
            propertyContainer.GetPropertyValueUntyped(property, searchInParent).HasValue();

        /// <summary>
        /// Sets property value if property is not set.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValueIfNotSet<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value)
        {
            var propertyValue = propertyContainer.GetPropertyValueUntyped(property, searchInParent: false);
            if (propertyValue == null || !propertyValue.HasValue())
                propertyContainer.SetValue(property, value);
        }
    }

    /// <summary>
    /// Search options.
    /// </summary>
    public readonly struct Search
    {
        public Func<IPropertyValue, bool> Condition { get; }

        public bool SearchInParent { get; }

        public Search(Func<IPropertyValue, bool> condition)
        {
            Condition = condition.AssertArgumentNotNull(nameof(condition));
            SearchInParent = true;
        }

        public Search(
            Search searchOptions,
            Func<IPropertyValue, bool> condition = null,
            bool? searchInParent = null)
        {
            Condition = condition ?? searchOptions.Condition;
            SearchInParent = searchInParent ?? searchOptions.SearchInParent;
        }

        public static Search ByProperty(IProperty property)
        {
            return new Search(default, condition: value => value.PropertyUntyped == property);
        }

        public static Search ByNameOrAlias(string name, bool ignoreCase = false)
        {
            return new Search(default, condition: propertyValue => propertyValue.IsMatchesByNameOrAlias(name, ignoreCase));
        }
    }

    public static class SearchBuilder
    {
        public static Search ByProperty(this Search search, IProperty property)
        {
            return new Search(search, condition: value => value.PropertyUntyped == property);
        }

        public static Search SearchInParent(this Search search, bool searchInParent = true)
        {
            return new Search(search, searchInParent: searchInParent);
        }

        public static Search ByNameOrAlias(this Search search, string name, bool ignoreCase = false)
        {
            return new Search(search, condition: propertyValue => propertyValue.IsMatchesByNameOrAlias(name, ignoreCase));
        }

        public static bool IsMatchesByNameOrAlias(this IPropertyValue propertyValue, string propertyName, bool ignoreCase = false)
        {
            StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (propertyName == null)
                return false;

            return propertyName.Equals(propertyValue.PropertyUntyped.Name, stringComparison)
                   || propertyName.Equals(propertyValue.PropertyUntyped.Alias, stringComparison);
        }
    }
}
