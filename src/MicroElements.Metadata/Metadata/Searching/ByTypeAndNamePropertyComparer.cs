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
        private readonly StringComparison _nameComparison;
        private readonly StringComparer _nameComparer;
        private readonly bool _ignoreTypeNullability;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByTypeAndNamePropertyComparer"/> class.
        /// </summary>
        /// <param name="nameComparison">StringComparison for property name comparing.</param>
        /// <param name="ignoreTypeNullability">Compare types ignore Nullable wrapper type.</param>
        public ByTypeAndNamePropertyComparer(
            StringComparison nameComparison = StringComparison.Ordinal,
            bool ignoreTypeNullability = false)
        {
            _nameComparison = nameComparison;
            _nameComparer = StringComparer.FromComparison(nameComparison);
            _ignoreTypeNullability = ignoreTypeNullability;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty? x, IProperty? y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            Type typeX = x.Type;
            Type typeY = y.Type;

            if (_ignoreTypeNullability)
            {
                if (typeX.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(typeX) is { } underlyingTypeX)
                    typeX = underlyingTypeX;
                if (typeY.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(typeY) is { } underlyingTypeY)
                    typeY = underlyingTypeY;
            }

            return typeX == typeY && x.Name.Equals(y.Name, _nameComparison);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty property)
        {
            string propertyName = property.Name;
            Type propertyType = property.Type;

            HashCode hashCode = default;
            hashCode.Add(propertyName, _nameComparer);

            if (_ignoreTypeNullability
                && propertyType.GetTypeInfo().IsValueType
                && Nullable.GetUnderlyingType(propertyType) is { } underlyingType)
            {
                propertyType = underlyingType;
            }

            hashCode.Add(propertyType);

            return hashCode.ToHashCode();
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
        public static IEqualityComparer<IProperty> Strict { get; } = new ByTypeAndNamePropertyComparer(nameComparison: StringComparison.Ordinal, ignoreTypeNullability: false);

        /// <summary>
        /// Gets property comparer by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> IgnoreNameCase { get; } = new ByTypeAndNamePropertyComparer(nameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: false);

        /// <summary>
        /// Gets property comparer by Type and Name ignoring name case and ignoring <see cref="Nullable{T}"/> wrapper.
        /// </summary>
        public static IEqualityComparer<IProperty> IgnoreNameCaseIgnoreNullability { get; } = new ByTypeAndNamePropertyComparer(nameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: true);
    }
}
