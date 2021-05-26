// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Compares properties by user defined predicate.
    /// </summary>
    public sealed class ByPredicatePropertyComparer : IEqualityComparer<IProperty>
    {
        private readonly Func<IProperty, IProperty, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByPredicatePropertyComparer"/> class.
        /// </summary>
        /// <param name="predicate">Predicate for comparing.</param>
        public ByPredicatePropertyComparer(Func<IProperty, IProperty, bool> predicate)
        {
            _predicate = predicate;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty? x, IProperty? y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            return _predicate(x, y);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty property) => HashCode.Combine(property.Name, property.Type);
    }
}
