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
    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, SearchCondition search)
        {
            // Ищем в своих свойствах.
            IPropertyValue propertyValue;
            if (search.Property != null)
            {
                // Search By Property
                propertyValue = propertyContainer.FirstOrDefault(pv => pv.PropertyUntyped == search.Property);
            }
            else
            {
                // Search By Name or Alias
                propertyValue = propertyContainer.FirstOrDefault(pv => pv.IsMatchesByNameOrAlias(search.Name, search.IgnoreCase));
            }

            if (propertyValue != null)
                return propertyValue;

            // Ищем у родителя
            if (search.SearchInParent && propertyContainer.ParentSource != null)
            {
                propertyValue = propertyContainer.ParentSource.GetPropertyValueUntyped(search);

                if (propertyValue != null)
                    return propertyValue;
            }

            return search.ReturnNotDefined ? PropertyValue.Create(search.Property ?? new Property<string>(search.Name), null, ValueSource.NotDefined) : null;
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="propertyName">Search conditions.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>value or null.</returns>
        public static object GetValueUntyped(this IPropertyContainer propertyContainer, string propertyName, bool searchInParent = true)
        {
            return propertyContainer
                .GetPropertyValueUntyped(new SearchCondition(propertyName, ignoreCase: true, searchInParent: searchInParent, returnNotDefined: false))
                ?.ValueUntyped;
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(this IPropertyContainer propertyContainer, SearchCondition search) =>
            (IPropertyValue<T>)propertyContainer.GetPropertyValueUntyped(search);

        /// <summary>
        /// Gets property and value by property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <param name="calculateValue">Calculate value if value was not found.</param>
        /// <param name="useDefaultValue">Use default value from property.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            bool searchInParent = true,
            bool calculateValue = true,
            bool useDefaultValue = true,
            bool returnNotDefined = true)
        {
            IPropertyValue<T> propertyValue = (IPropertyValue<T>)propertyContainer.GetPropertyValueUntyped(
                new SearchCondition(property, searchInParent: searchInParent, returnNotDefined: false));

            if (propertyValue.HasValue())
                return propertyValue;

            // Свойство не в списке свойств, но может его можно вычислить.
            if (calculateValue && property.Calculate != null)
            {
                var calculatedValue = property.Calculate(propertyContainer);
                return new PropertyValue<T>(property, calculatedValue, ValueSource.Calculated);
            }

            // Вернем значение по умолчанию.
            if (useDefaultValue)
                return new PropertyValue<T>(property, property.DefaultValue(), ValueSource.DefaultValue);

            return returnNotDefined ? new PropertyValue<T>(property, default, ValueSource.NotDefined) : null;
        }

        /// <summary>
        /// Gets property and value by property.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, IProperty property, bool searchInParent = true) =>
            propertyContainer.GetPropertyValueUntyped(new SearchCondition(property, searchInParent: searchInParent, returnNotDefined: true));

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
            if (propertyValue.IsNullOrNotDefined())
                propertyContainer.SetValue(property, value);
        }
    }

    /// <summary>
    /// Search options.
    /// </summary>
    public readonly struct SearchCondition
    {
        /// <summary>
        /// Property to search.
        /// </summary>
        public readonly IProperty Property;

        /// <summary>
        /// Search by name and alias.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Name search ignore case.
        /// </summary>
        public readonly bool IgnoreCase;

        /// <summary>
        /// >Do search in parent.
        /// </summary>
        public readonly bool SearchInParent;

        /// <summary>
        /// Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.
        /// </summary>
        public readonly bool ReturnNotDefined;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCondition"/> struct.
        /// </summary>
        /// <param name="property">Search by property.</param>
        /// <param name="searchInParent">Do search in parent.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        public SearchCondition(IProperty property, bool searchInParent, bool returnNotDefined = true)
        {
            Property = property.AssertArgumentNotNull(nameof(property));

            Name = null;
            IgnoreCase = false;

            SearchInParent = searchInParent;
            ReturnNotDefined = returnNotDefined;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCondition"/> struct.
        /// Search by name.
        /// </summary>
        /// <param name="name">Search by name.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <param name="searchInParent">Do search in parent.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        public SearchCondition(string name, bool ignoreCase, bool searchInParent, bool returnNotDefined = true)
        {
            Property = null;

            Name = name;
            IgnoreCase = ignoreCase;

            SearchInParent = searchInParent;
            ReturnNotDefined = returnNotDefined;
        }
    }

    /// <summary>
    /// Extension methods for search.
    /// </summary>
    public static class SearchExtensions
    {
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
