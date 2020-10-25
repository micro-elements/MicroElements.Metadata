// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Search helpers.
    /// </summary>
    [DebuggerStepThrough]
    public static class Search
    {
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
        /// Creates search condition by name or alias.
        /// </summary>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameOrAlias(string name, bool ignoreCase = false)
            => new SearchOptions(new Property<UntypedSearch>(name), PropertyComparer.ByNameOrAlias(ignoreCase));

        /// <summary>
        /// Creates search condition by name or alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameOrAlias<T>(string name, bool ignoreCase = false)
            => new SearchOptions(new Property<T>(name), PropertyComparer.ByNameOrAlias(ignoreCase));

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
                searchProperty: new Property<T>(name),
                propertyComparer: propertyComparer);

        /// <summary>
        /// Copy with override one ore more fields.
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="property">Property to search.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <param name="searchInParent">Do search in parent.</param>
        /// <param name="calculateValue">Calculate value if value was not found.</param>
        /// <param name="useDefaultValue">Use default value from property is property value was not found.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions With(
            this in SearchOptions searchOptions,
            IProperty property = null,
            IEqualityComparer<IProperty> propertyComparer = null,
            bool? searchInParent = null,
            bool? calculateValue = null,
            bool? useDefaultValue = null,
            bool? returnNotDefined = null)
        {
            return new SearchOptions(
                searchProperty: property ?? searchOptions.SearchProperty,
                propertyComparer: propertyComparer ?? searchOptions.PropertyComparer ?? PropertyComparer.DefaultEqualityComparer,
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
            => searchOptions.With(useDefaultValue: useDefaultValue);

        /// <summary>
        /// <inheritdoc cref="SearchOptions.ReturnNotDefined"/>
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <param name="returnNotDefined"><see cref="SearchOptions.ReturnNotDefined"/> value.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ReturnNotDefined(this in SearchOptions searchOptions, bool returnNotDefined = true)
            => searchOptions.With(returnNotDefined: returnNotDefined);

        /// <summary>
        /// Returns null if property was not found.
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/> instance.</param>
        /// <returns>New <see cref="SearchOptions"/> instance.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ReturnNull(this in SearchOptions searchOptions)
            => searchOptions.With(returnNotDefined: false);
    }
}
