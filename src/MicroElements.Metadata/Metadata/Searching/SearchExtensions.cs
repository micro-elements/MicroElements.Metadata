// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.CodeContracts;
using MicroElements.Reflection.CodeCompiler;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extension methods for search.
    /// </summary>
    public static partial class SearchExtensions
    {
        /// <summary>
        /// Gets or calculates typed property and value for property using search conditions.
        /// It's a full search that uses all search options: <see cref="SearchOptions.SearchInParent"/>, <see cref="SearchOptions.CalculateValue"/>,
        /// <see cref="SearchOptions.UseDefaultValue"/>, <see cref="SearchOptions.ReturnNotDefined"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null according option <see cref="SearchOptions.ReturnNotDefined"/>.</returns>
        public static IPropertyValue<T>? GetPropertyValue<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? search = default)
        {
            return Search.Algorithm.GetPropertyValue(propertyContainer, property, search);
        }

        /// <summary>
        /// Searches property and value for untyped property using search conditions.
        /// Search does not use <see cref="SearchOptions.UseDefaultValue"/> and <see cref="SearchOptions.CalculateValue"/>.
        /// Search uses only <see cref="SearchOptions.SearchInParent"/> and <see cref="SearchOptions.ReturnNotDefined"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null according option <see cref="SearchOptions.ReturnNotDefined"/>.</returns>
        public static IPropertyValue? SearchPropertyValueUntyped(
            this IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? search = default)
        {
            return Search.Algorithm.SearchPropertyValueUntyped(propertyContainer, property, search);
        }

        /// <summary>
        /// Gets property and value for untyped property using search conditions.
        /// Uses simple untyped search `SearchPropertyValueUntyped` if CanUseSimpleUntypedSearch or `property` has type <see cref="Search.UntypedSearch"/>.
        /// Uses full `GetPropertyValue{T}` based on property.Type in other cases.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue? GetPropertyValueUntyped(
            this IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? search = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            if ((search ?? propertyContainer.SearchOptions).CanUseSimpleUntypedSearch() || property.Type == typeof(Search.UntypedSearch))
            {
                return propertyContainer.SearchPropertyValueUntyped(property, search);
            }

            return GetPropertyValueUntypedFull(propertyContainer, property, search);

            static IPropertyValue? GetPropertyValueUntypedFull(IPropertyContainer propertyContainer, IProperty property, SearchOptions? search)
            {
                static IPropertyValue? GetPropertyValueAdapter<T>(IPropertyContainer pc, IProperty p, SearchOptions? s) => pc.GetPropertyValue((IProperty<T>)p, s);
                var getPropertyValue = CodeCompiler.CachedCompiledFunc<IPropertyContainer, IProperty, SearchOptions?, IPropertyValue?>(property.Type, "GetPropertyValue", GetPropertyValueAdapter<CodeCompiler.GenericType>);
                return getPropertyValue(propertyContainer, property, search);
            }
        }

        /// <summary>
        /// Gets or calculates value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <param name="defaultValue">Default value that returns if property value was not found.</param>
        /// <returns>The value for property.</returns>
        public static T? GetValue<T>(
            this IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? search = null,
            T? defaultValue = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, search);
            return propertyValue.HasValue() ? propertyValue.Value : defaultValue;
        }

        /// <summary>
        /// Gets or calculates untyped value for property.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <returns>The value for property.</returns>
        public static object? GetValueUntyped(
            this IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? search = null)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            IPropertyValue? propertyValue = propertyContainer.GetPropertyValueUntyped(property, search);
            return propertyValue.HasValue() ? propertyValue.ValueUntyped : null;
        }

        /// <summary>
        /// Gets or calculates optional not null value.
        /// Returns option in <see cref="MicroElements.Functional.OptionState.Some"/> state if property value exists and not null.
        /// Returns None if value was not found.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search options.</param>
        /// <returns>Optional property value.</returns>
        /// TODO: Migrate
        // public static MicroElements.Functional.Option<T> GetValueAsOption<T>(
        //     this IPropertyContainer propertyContainer,
        //     IProperty<T> property,
        //     SearchOptions? search = null)
        // {
        //     propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
        //     property.AssertArgumentNotNull(nameof(property));
        //
        //     IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, search);
        //     if (propertyValue.HasValue() && !propertyValue.Value.IsNull())
        //     {
        //         return propertyValue.Value;
        //     }
        //
        //     return MicroElements.Functional.Option<T>.None;
        // }

        /// <summary>
        /// Gets property and value by search conditions.
        /// NOTE: <see cref="SearchOptions.SearchProperty"/> must be set in <paramref name="search"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue<T>? GetPropertyValue<T>(this IPropertyContainer propertyContainer, SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");
            if (search.SearchProperty.Type != typeof(T))
                throw new InvalidOperationException($"SearchProperty type must should be {typeof(T)} but is {search.SearchProperty.Type}.");

            return propertyContainer.GetPropertyValue((IProperty<T>)search.SearchProperty, search);
        }

        /// <summary>
        /// Searches property and value by search conditions.
        /// NOTE: <see cref="SearchOptions.SearchProperty"/> must be set in <paramref name="search"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue? SearchPropertyValueUntyped(this IPropertyContainer propertyContainer, SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");

            return propertyContainer.SearchPropertyValueUntyped(search.SearchProperty, search);
        }

        /// <summary>
        /// Gets property and value by search conditions.
        /// NOTE: <see cref="SearchOptions.SearchProperty"/> must be set in <paramref name="search"/>.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns><see cref="IPropertyValue"/> or null.</returns>
        public static IPropertyValue? GetPropertyValueUntyped(this IPropertyContainer propertyContainer, [DisallowNull] SearchOptions search)
        {
            if (search.SearchProperty == null)
                throw new InvalidOperationException("SearchProperty must be set in SearchOptions.");

            return propertyContainer.GetPropertyValueUntyped(search.SearchProperty, search);
        }

        /// <summary>
        /// Gets or calculates value by name.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="propertyName">Search conditions.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>value or null.</returns>
        [return: MaybeNull]
        public static T GetValueByName<T>(this IPropertyContainer propertyContainer, string propertyName, bool searchInParent = true)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            propertyName.AssertArgumentNotNull(nameof(propertyName));

            IPropertyValue<T> propertyValue = propertyContainer.GetPropertyValue<T>(Search
                .ByNameOrAlias<T>(propertyName, ignoreCase: true)
                .SearchInParent(searchInParent)
                .ReturnNotDefined())!;

            return propertyValue.Value;
        }

        /// <summary>
        /// Gets or calculates value by name.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="propertyName">Search conditions.</param>
        /// <param name="searchInParent">Search in parent.</param>
        /// <returns>value or null.</returns>
        public static object? GetValueUntypedByName(this IPropertyContainer propertyContainer, string propertyName, bool searchInParent = true)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            propertyName.AssertArgumentNotNull(nameof(propertyName));

            IPropertyValue? propertyValue = propertyContainer.GetPropertyValueUntyped(Search
                .ByNameOrAlias(propertyName, ignoreCase: true)
                .SearchInParent(searchInParent)
                .ReturnNull());

            return propertyValue?.ValueUntypedOrNull();
        }

        /// <summary>
        /// Returns true if simple untyped search can be used.
        /// </summary>
        /// <param name="search">SearchOptions.</param>
        /// <returns>true if untyped search can be used.</returns>
        public static bool CanUseSimpleUntypedSearch(this in SearchOptions search) =>
            search.CalculateValue == false && search.UseDefaultValue == false;

        /// <summary>
        /// Returns true if <paramref name="propertyContainer"/> has property.
        /// Also searches in parent sources.
        /// </summary>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="search">Search conditions.</param>
        /// <returns>True if property exists in container.</returns>
        public static bool HasValue(
            this IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? search = null)
        {
            return propertyContainer.GetPropertyValueUntyped(property, search).HasValue();
        }

        /// <summary>
        /// Returns true if <paramref name="property"/> Name or Alias equals to <paramref name="name"/>.
        /// </summary>
        /// <param name="property">Property instance.</param>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">IgnoreCase flag.</param>
        /// <returns>compare result.</returns>
        public static bool IsMatchesByNameOrAlias(
            this IProperty? property,
            string? name,
            bool ignoreCase = false)
        {
            StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (property == null || name == null)
                return false;

            return name.Equals(property.Name, stringComparison) || name.Equals(property.Alias, stringComparison);
        }
    }
}
