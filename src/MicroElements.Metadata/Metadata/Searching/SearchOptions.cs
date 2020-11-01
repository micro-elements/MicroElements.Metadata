// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Search options.
    /// </summary>
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Ok.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "Ok.")]
    public readonly struct SearchOptions : IEquatable<SearchOptions>
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

        private readonly IProperty? _searchProperty;
        private readonly IEqualityComparer<IProperty>? _propertyComparer;
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
        public IProperty? SearchProperty => _searchProperty;

        /// <summary>
        /// Equality comparer for comparing properties.
        /// </summary>
        public IEqualityComparer<IProperty> PropertyComparer => _propertyComparer ?? Metadata.PropertyComparer.DefaultEqualityComparer;

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
        /// Return fake PropertyValue with <see cref="ValueSource.NotDefined"/> and Value set to default if no PropertyValue was found.
        /// Returns null if ReturnNotDefined is false.
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
            IProperty? searchProperty = null,
            IEqualityComparer<IProperty>? propertyComparer = null,
            bool searchInParent = true,
            bool calculateValue = true,
            bool useDefaultValue = true,
            bool returnNotDefined = true)
        {
            _searchProperty = searchProperty;
            _propertyComparer = propertyComparer ?? Metadata.PropertyComparer.DefaultEqualityComparer;
            _searchInParent = searchInParent;
            _calculateValue = calculateValue;
            _useDefaultValue = useDefaultValue;
            _returnNotDefined = returnNotDefined;
        }

        /// <summary>
        /// Returns true if options is default.
        /// </summary>
        /// <returns>True if options is default.</returns>
        public bool IsDefault() => this == default;

        /// <inheritdoc />
        public bool Equals(SearchOptions other) =>
            Equals(_searchProperty, other._searchProperty)
            && Equals(_propertyComparer, other._propertyComparer)
            && _searchInParent == other._searchInParent
            && _calculateValue == other._calculateValue
            && _useDefaultValue == other._useDefaultValue
            && _returnNotDefined == other._returnNotDefined;

        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            obj is SearchOptions other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() =>
            HashCode.Combine(_searchProperty, _propertyComparer, _searchInParent, _calculateValue, _useDefaultValue, _returnNotDefined);

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">Left options.</param>
        /// <param name="right">Right options.</param>
        /// <returns>True if options are equal.</returns>
        public static bool operator ==(SearchOptions left, SearchOptions right) => left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">Left options.</param>
        /// <param name="right">Right options.</param>
        /// <returns>True if options are not equal.</returns>
        public static bool operator !=(SearchOptions left, SearchOptions right) => !left.Equals(right);
    }
}
