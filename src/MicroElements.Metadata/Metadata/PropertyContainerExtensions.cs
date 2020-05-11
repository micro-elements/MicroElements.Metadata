// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        /// Searches property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue SearchPropertyValueUntyped(this IPropertyContainer propertyContainer, IProperty property, SearchOptions search = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            // Search property by EqualityComparer
            IPropertyValue propertyValue = propertyContainer.FirstOrDefault(pv => search.PropertyComparer.Equals(pv.PropertyUntyped, property));

            if (propertyValue != null)
                return propertyValue;

            // Search in parent
            if (search.SearchInParent && propertyContainer.ParentSource != null)
            {
                propertyValue = propertyContainer.ParentSource.SearchPropertyValueUntyped(property, search);

                if (propertyValue != null)
                    return propertyValue;
            }

            return search.ReturnNotDefined ? PropertyValue.Create(property, property.Type.GetDefaultValue(), ValueSource.NotDefined) : null;
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(this IPropertyContainer propertyContainer, IProperty<T> property, SearchOptions search = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            // Base search
            IPropertyValue propertyValue = propertyContainer
                .SearchPropertyValueUntyped(property, search
                    .UseDefaultValue(false)
                    .ReturnNull());

            // Good job - return result!
            if (propertyValue != null)
                return (IPropertyValue<T>)propertyValue;

            // Property can be calculated.
            if (search.CalculateValue && property.Calculate != null)
            {
                var calculatedValue = property.Calculate(propertyContainer);
                return new PropertyValue<T>(property, calculatedValue, ValueSource.Calculated);
            }

            // Maybe default value?
            if (search.UseDefaultValue)
                return new PropertyValue<T>(property, property.DefaultValue(), ValueSource.DefaultValue);

            // Return null or NotDefined
            return search.ReturnNotDefined ? new PropertyValue<T>(property, default, ValueSource.NotDefined) : null;
        }

        /// <summary>
        /// Calls <see cref="GetPropertyValue{T}(IPropertyContainer,SearchOptions)"/> using type from <paramref name="property"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueCompiled(this IPropertyContainer propertyContainer, IProperty property, SearchOptions search)
        {
            static IPropertyValue GetPropertyValueAdapter<T>(IPropertyContainer propertyContainer, IProperty property, SearchOptions search) => GetPropertyValue(propertyContainer, (IProperty<T>)property, search);
            var getPropertyValueCompiled = CodeCompiler.CachedCompiledFunc<IPropertyContainer, IProperty, SearchOptions, IPropertyValue>(property.Type, GetPropertyValueAdapter<CodeCompiler.GenericType>);
            return getPropertyValueCompiled(propertyContainer, property, search);
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, IProperty property, SearchOptions search = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            if (search.CanUseUntypedSearch() || property.Type == typeof(Search.UntypedSearch))
            {
                return propertyContainer.SearchPropertyValueUntyped(property, search);
            }

            return propertyContainer.GetPropertyValueCompiled(property, search);
        }

        /// <summary>
        /// Returns true if untyped search can be used.
        /// </summary>
        /// <param name="search">SearchOptions.</param>
        /// <returns>true if untyped search can be used.</returns>
        public static bool CanUseUntypedSearch(this in SearchOptions search) => search.CalculateValue == false && search.UseDefaultValue == false;

        /// <summary>
        /// Searches property and value by search conditions.
        /// NOTE: PropertyToSearch must be set in <paramref name="search"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue SearchPropertyValueUntyped(this IPropertyContainer propertyContainer, SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");

            return propertyContainer.SearchPropertyValueUntyped(search.SearchProperty, search);
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// NOTE: PropertyToSearch must be set in <paramref name="search"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");

            return propertyContainer.GetPropertyValueUntyped(search.SearchProperty, search);
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// NOTE: PropertyToSearch must be set in <paramref name="search"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(this IPropertyContainer propertyContainer, SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");
            if (search.SearchProperty.Type != typeof(T))
                throw new InvalidOperationException($"SearchProperty type must should be {typeof(T)} but is {search.SearchProperty.Type}.");

            return propertyContainer.GetPropertyValue((IProperty<T>)search.SearchProperty, search);
        }

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
        /// <param name="propertyComparer">Property equality comparer.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T> GetPropertyValue<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            bool searchInParent = true,
            bool calculateValue = true,
            bool useDefaultValue = true,
            bool returnNotDefined = true,
            IEqualityComparer<IProperty> propertyComparer = null)
        {
            return propertyContainer.GetPropertyValue(
                property, new SearchOptions(
                    propertyComparer: propertyComparer,
                    searchInParent: searchInParent,
                    calculateValue: calculateValue,
                    useDefaultValue: useDefaultValue,
                    returnNotDefined: returnNotDefined));
        }

        /// <summary>
        /// Gets property and value by property.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue GetPropertyValueUntyped(this IPropertyContainer propertyContainer, IProperty property, bool searchInParent = true)
            => propertyContainer.GetPropertyValueUntyped(property, new SearchOptions(searchInParent: searchInParent, returnNotDefined: true));

        /// <summary>
        /// Gets property and value by search conditions.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="propertyName">Search conditions.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>value or null.</returns>
        public static object GetValueUntyped(this IPropertyContainer propertyContainer, string propertyName, bool searchInParent = true)
        {
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(Search
                .ByNameOrAlias(propertyName, ignoreCase: true)
                .SearchInParent(searchInParent)
                .ReturnNull());

            return propertyValue?.ValueUntyped;
        }

        /// <summary>
        /// Returns true if <paramref name="propertyContainer"/> has property.
        /// Also searches in parent sources.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue(this IPropertyContainer propertyContainer, IProperty property, SearchOptions search) =>
            propertyContainer.GetPropertyValueUntyped(property, search).HasValue();

        /// <summary>
        /// Sets property value if property is not set.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValueIfNotSet<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value)
        {
            var propertyValue = propertyContainer.GetPropertyValueUntyped(property, SearchOptions.Default.SearchInParent(false).ReturnNull());
            if (propertyValue.IsNullOrNotDefined())
                propertyContainer.SetValue(property, value);
        }
    }
}
