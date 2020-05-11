// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
            => new SearchOptions(new Property<UntypedSearch>(name), Property.ByNameOrAlias(ignoreCase));

        /// <summary>
        /// Creates search condition by name or alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">Comparison.</param>
        /// <returns>SearchCondition.</returns>
        [DebuggerStepThrough]
        public static SearchOptions ByNameOrAlias<T>(string name, bool ignoreCase = false)
            => new SearchOptions(new Property<T>(name), Property.ByNameOrAlias(ignoreCase));

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
                propertyComparer: propertyComparer ?? searchOptions.PropertyComparer ?? Property.DefaultEqualityComparer,
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

    /// <summary>
    /// Search options.
    /// </summary>
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Ok.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "Ok.")]
    public readonly struct SearchOptions
    {
        /*-------------------------------------------*/
        /*        Predefined SearchOptions           */
        /*-------------------------------------------*/

        /// <summary>
        /// Default search options: (SearchInParent(), CalculateValue(), UseDefaultValue(), ReturnNotDefined()).
        /// </summary>
        public static readonly SearchOptions Default = default(SearchOptions)
            .SearchInParent()
            .CalculateValue()
            .UseDefaultValue()
            .ReturnNotDefined();

        /// <summary>
        /// Search only existing properties in property container: (SearchInParent(false), CalculateValue(false), UseDefaultValue(false), ReturnNull()).
        /// </summary>
        public static readonly SearchOptions ExistingOnly = default(SearchOptions)
            .SearchInParent(false)
            .CalculateValue(false)
            .UseDefaultValue(false)
            .ReturnNull();

        /// <summary>
        /// Search only existing properties in property container including searching in parent.
        /// </summary>
        public static readonly SearchOptions ExistingOnlyWithParent = ExistingOnly.SearchInParent(true);

        /*-------------------------------------------*/
        /*           Private fields                  */
        /*-------------------------------------------*/

        private readonly IProperty _searchProperty;
        private readonly IEqualityComparer<IProperty> _propertyComparer;
        private readonly bool? _searchInParent;
        private readonly bool? _calculateValue;
        private readonly bool? _useDefaultValue;
        private readonly bool? _returnNotDefined;

        /*-------------------------------------------*/
        /*           Properties                      */
        /*-------------------------------------------*/

        /// <summary>
        /// Property to search. Used for by name search.
        /// </summary>
        public IProperty SearchProperty => _searchProperty;

        /// <summary>
        /// Equality comparer for comparing properties.
        /// </summary>
        public IEqualityComparer<IProperty> PropertyComparer => _propertyComparer ?? Property.DefaultEqualityComparer;

        /// <summary>
        /// Do search in parent if no PropertyValue was found.
        /// </summary>
        public bool SearchInParent => _searchInParent ?? true;

        /// <summary>
        /// Calculate value if value was not found.
        /// </summary>
        public bool CalculateValue => _calculateValue ?? true;

        /// <summary>
        /// Use default value from property is property value was not found.
        /// </summary>
        public bool UseDefaultValue => _useDefaultValue ?? true;

        /// <summary>
        /// Return fake not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found. If ReturnNotDefined is false then null will be returned.
        /// </summary>
        public bool ReturnNotDefined => _returnNotDefined ?? true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchOptions"/> struct.
        /// </summary>
        /// <param name="searchProperty">Property to search.</param>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <param name="searchInParent">Do search in parent.</param>
        /// <param name="calculateValue">Calculate value if value was not found.</param>
        /// <param name="useDefaultValue">Use default value from property is property value was not found.</param>
        /// <param name="returnNotDefined">Return not null PropertyValue with <see cref="ValueSource.NotDefined"/> if no PropertyValue was found.</param>
        public SearchOptions(
            IProperty searchProperty = null,
            IEqualityComparer<IProperty> propertyComparer = null,
            bool searchInParent = true,
            bool calculateValue = true,
            bool useDefaultValue = true,
            bool returnNotDefined = true)
        {
            _searchProperty = searchProperty;
            _propertyComparer = propertyComparer ?? Property.DefaultEqualityComparer;
            _searchInParent = searchInParent;
            _calculateValue = calculateValue;
            _useDefaultValue = useDefaultValue;
            _returnNotDefined = returnNotDefined;
        }
    }

    /// <summary>
    /// Property comparer by reference equality.
    /// </summary>
    public sealed class ByReferenceEqualityComparer : IEqualityComparer<IProperty>
    {
        /// <inheritdoc/>
        public bool Equals(IProperty x, IProperty y) => ReferenceEquals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(IProperty obj) => HashCode.Combine(obj);
    }

    /// <summary>
    /// Property comparer by <see cref="IProperty.Name"/> and <see cref="IProperty.Type"/>.
    /// </summary>
    public sealed class ByNameAndTypeEqualityComparer : IEqualityComparer<IProperty>
    {
        /// <inheritdoc/>
        public bool Equals(IProperty x, IProperty y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && x.Type == y.Type;
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty obj) => HashCode.Combine(obj.Name, obj.Type);
    }

    /// <summary>
    /// Property comparer by <see cref="IProperty.Name"/> or <see cref="IProperty.Alias"/>.
    /// </summary>
    public sealed class ByNameOrAliasEqualityComparer : IEqualityComparer<IProperty>
    {
        /// <summary>
        /// Compare names using ordinal (binary) sort rules and ignoring the case of the strings being compared.
        /// </summary>
        public static readonly ByNameOrAliasEqualityComparer IgnoreCase = new ByNameOrAliasEqualityComparer(true);

        /// <summary>
        /// Compare names using ordinal (binary) sort rules.
        /// </summary>
        public static readonly ByNameOrAliasEqualityComparer Ordinal = new ByNameOrAliasEqualityComparer(false);

        private readonly bool _ignoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByNameOrAliasEqualityComparer"/> class.
        /// </summary>
        /// <param name="ignoreCase">Ignore case for compare.</param>
        public ByNameOrAliasEqualityComparer(bool ignoreCase)
        {
            _ignoreCase = ignoreCase;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty x, IProperty y)
        {
            return x.IsMatchesByNameOrAlias(y?.Name, _ignoreCase) || x.IsMatchesByNameOrAlias(y?.Alias, _ignoreCase);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty obj) => HashCode.Combine(obj.Name);
    }

    /// <summary>
    /// Extension methods for search.
    /// </summary>
    public static class SearchExtensions
    {
        /// <summary>
        /// Returns true if <paramref name="property"/> Name or Alias equals to <paramref name="name"/>.
        /// </summary>
        /// <param name="property">Property instance.</param>
        /// <param name="name">Name to search.</param>
        /// <param name="ignoreCase">IgnoreCase flag.</param>
        /// <returns>compare result.</returns>
        public static bool IsMatchesByNameOrAlias([AllowNull] this IProperty property, [AllowNull] string name, bool ignoreCase = false)
        {
            StringComparison stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (property == null || name == null)
                return false;

            return name.Equals(property.Name, stringComparison) || name.Equals(property.Alias, stringComparison);
        }
    }
}
