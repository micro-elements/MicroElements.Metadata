// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparer by Type and Name.
    /// </summary>
    public sealed partial class ByTypeAndNamePropertyComparer : IEqualityComparer<IProperty>
    {
        private readonly StringComparison _typeNameComparison;
        private readonly bool _ignoreTypeNullability;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByTypeAndNamePropertyComparer"/> class.
        /// </summary>
        /// <param name="typeNameComparison">StringComparison for property name comparing.</param>
        /// <param name="ignoreTypeNullability">Compare types ignore Nullable wrapper type.</param>
        public ByTypeAndNamePropertyComparer(
            StringComparison typeNameComparison = StringComparison.Ordinal,
            bool ignoreTypeNullability = false)
        {
            _typeNameComparison = typeNameComparison;
            _ignoreTypeNullability = ignoreTypeNullability;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty? x, IProperty? y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            Type typeX = _ignoreTypeNullability && x.Type.GetTypeInfo().IsValueType ? Nullable.GetUnderlyingType(x.Type) ?? x.Type : x.Type;
            Type typeY = _ignoreTypeNullability && y.Type.GetTypeInfo().IsValueType ? Nullable.GetUnderlyingType(y.Type) ?? y.Type : y.Type;

            return typeX == typeY && x.Name.Equals(y.Name, _typeNameComparison);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty property)
        {
            string propertyName = property.Name;
            Type propertyType = property.Type;

            if (_typeNameComparison == StringComparison.OrdinalIgnoreCase ||
                _typeNameComparison == StringComparison.InvariantCultureIgnoreCase ||
                _typeNameComparison == StringComparison.CurrentCultureIgnoreCase)
                propertyName = propertyName.ToLower();

            if (_ignoreTypeNullability
                && propertyType.GetTypeInfo().IsValueType
                && Nullable.GetUnderlyingType(propertyType) is { } underlyingType)
            {
                propertyType = underlyingType;
            }

            return HashCode.Combine(propertyName, propertyType);
        }
    }

    /// <summary>
    /// Property comparer by Type and Name.
    /// </summary>
    public sealed partial class ByTypeAndNamePropertyComparer
    {
        /// <summary>
        /// Gets property comparer by Type and Name.
        /// </summary>
        public static IEqualityComparer<IProperty> Strict { get; } = new ByTypeAndNamePropertyComparer(typeNameComparison: StringComparison.Ordinal, ignoreTypeNullability: false);

        /// <summary>
        /// Gets property comparer by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> IgnoreNameCase { get; } = new ByTypeAndNamePropertyComparer(typeNameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: false);

        /// <summary>
        /// Gets property comparer by Type and Name ignoring name case and ignoring <see cref="Nullable{T}"/> wrapper.
        /// </summary>
        public static IEqualityComparer<IProperty> IgnoreNameCaseIgnoreNullability { get; } = new ByTypeAndNamePropertyComparer(typeNameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: true);
    }
}
