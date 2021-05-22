// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparer by Type and Name.
    /// </summary>
    public sealed class ByTypeAndNameEqualityComparer : IEqualityComparer<IProperty>
    {
        private readonly StringComparison _stringComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByTypeAndNameEqualityComparer"/> class.
        /// </summary>
        /// <param name="stringComparison">StringComparison for name comparing.</param>
        public ByTypeAndNameEqualityComparer(StringComparison stringComparison = StringComparison.Ordinal)
        {
            _stringComparison = stringComparison;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty? x, IProperty? y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            return x.Type == y.Type && x.Name.Equals(y.Name, _stringComparison);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty property) => HashCode.Combine(property.Name, property.Type);
    }
}
