// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
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
}
