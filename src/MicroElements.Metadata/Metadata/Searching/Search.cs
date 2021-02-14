// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Search helpers.
    /// </summary>
    [DebuggerStepThrough]
    public static class Search
    {
        private static ISearchAlgorithm? _algorithm = DefaultSearchAlgorithm.Instance;

        /// <summary>
        /// Gets or Sets default <see cref="ISearchAlgorithm"/> for all extension methods.
        /// </summary>
        [NotNull]
        public static ISearchAlgorithm Algorithm
        {
            get => _algorithm ?? DefaultSearchAlgorithm.Instance;
            set => _algorithm = value;
        }

        /// <summary>
        /// Type used for untyped named search.
        /// </summary>
        internal sealed class UntypedSearch { }

        /// <summary>
        /// <inheritdoc cref="SearchOptions.Default"/>.
        /// </summary>
        public static readonly SearchOptions Default = SearchOptions.Default;

        /// <summary>
        /// <inheritdoc cref="SearchOptions.ExistingOnly"/>.
        /// </summary>
        public static readonly SearchOptions ExistingOnly = SearchOptions.ExistingOnly;

        /// <summary>
        /// <inheritdoc cref="SearchOptions.ExistingOnlyWithParent"/>.
        /// </summary>
        public static readonly SearchOptions ExistingOnlyWithParent = SearchOptions.ExistingOnlyWithParent;

        /// <summary>
        /// Cached properties for internal search use.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        internal static class CachedProperty<T>
        {
            private static readonly ConcurrentDictionary<string, IProperty<T>> _propertyCache = new ConcurrentDictionary<string, IProperty<T>>();

            public static IProperty<T> ByName(string name) => _propertyCache.GetOrAdd(name, n => new Property<T>(n));
        }

        /// <summary>
        /// Creates search condition by name or alias.
        /// </summary>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameOrAlias(string name, bool ignoreCase = false)
            => new SearchOptions(
                searchProperty: CachedProperty<UntypedSearch>.ByName(name),
                propertyComparer: PropertyComparer.ByNameOrAlias(ignoreCase));

        /// <summary>
        /// Creates search condition by name or alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameOrAlias<T>(string name, bool ignoreCase = false)
            => new SearchOptions(
                searchProperty: CachedProperty<T>.ByName(name),
                propertyComparer: PropertyComparer.ByNameOrAlias(ignoreCase));

        /// <summary>
        /// Creates search condition by type and name.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Name to search.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameAndComparer<T>(string name, IEqualityComparer<IProperty> propertyComparer)
            => new SearchOptions(
                searchProperty: CachedProperty<T>.ByName(name),
                propertyComparer: propertyComparer);

        /// <summary>
        /// Creates search condition by type and name.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="searchOptions">Source search options.</param>
        /// <param name="name">Name to search.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions UseSearchByNameAndComparer<T>(this in SearchOptions searchOptions, string name, IEqualityComparer<IProperty> propertyComparer)
            => searchOptions.With(
                searchProperty: CachedProperty<T>.ByName(name),
                propertyComparer: propertyComparer);

        /// <summary>
        /// Copy with override one ore more fields.
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="searchProperty">Property to search.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <param name="searchInParent">Do search in parent.</param>
        /// <param name="calculateValue">Calculate value if value was not found.</param>
        /// <param name="useDefaultValue">Use default value from property is property value was not found.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions With(
            this in SearchOptions searchOptions,
            IProperty? searchProperty = null,
            IEqualityComparer<IProperty>? propertyComparer = null,
            bool? searchInParent = null,
            bool? calculateValue = null,
            bool? useDefaultValue = null,
            bool? returnNotDefined = null)
        {
            return new SearchOptions(
                searchProperty: searchProperty ?? searchOptions.SearchProperty,
                propertyComparer: propertyComparer ?? searchOptions.PropertyComparer,
                searchInParent: searchInParent ?? searchOptions.SearchInParent,
                calculateValue: calculateValue ?? searchOptions.CalculateValue,
                useDefaultValue: useDefaultValue ?? searchOptions.UseDefaultValue,
                returnNotDefined: returnNotDefined ?? searchOptions.ReturnNotDefined);
        }

        /// <summary>
        /// <inheritdoc cref="SearchOptions.PropertyComparer"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="propertyComparer"><see cref="SearchOptions.PropertyComparer"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions WithPropertyComparer(this in SearchOptions searchOptions, IEqualityComparer<IProperty> propertyComparer)
            => searchOptions.With(propertyComparer: propertyComparer);

        /// <summary>
        /// <inheritdoc cref="SearchOptions.SearchInParent"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="searchInParent"><see cref="SearchOptions.SearchInParent"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions SearchInParent(this in SearchOptions searchOptions, bool searchInParent = true)
            => searchOptions.With(searchInParent: searchInParent);

        /// <summary>
        /// <inheritdoc cref="SearchOptions.CalculateValue"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="calculateValue"><see cref="SearchOptions.CalculateValue"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions CalculateValue(this in SearchOptions searchOptions, bool calculateValue = true)
            => searchOptions.With(calculateValue: calculateValue);

        /// <summary>
        /// <inheritdoc cref="SearchOptions.UseDefaultValue"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="useDefaultValue"><see cref="SearchOptions.UseDefaultValue"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions UseDefaultValue(this in SearchOptions searchOptions, bool useDefaultValue = true)
            => useDefaultValue != searchOptions.UseDefaultValue ? searchOptions.With(useDefaultValue: useDefaultValue) : searchOptions;

        /// <summary>
        /// <inheritdoc cref="SearchOptions.ReturnNotDefined"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="returnNotDefined"><see cref="SearchOptions.ReturnNotDefined"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ReturnNotDefined(this in SearchOptions searchOptions, bool returnNotDefined = true)
            => searchOptions.ReturnNotDefined != returnNotDefined ? searchOptions.With(returnNotDefined: returnNotDefined) : searchOptions;

        /// <summary>
        /// Returns null if property was not found.
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ReturnNull(this in SearchOptions searchOptions)
            => searchOptions.ReturnNotDefined ? searchOptions.With(returnNotDefined: false) : searchOptions;
    }
}
