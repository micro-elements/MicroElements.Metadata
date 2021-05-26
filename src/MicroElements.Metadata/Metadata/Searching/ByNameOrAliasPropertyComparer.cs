// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/>.
    /// </summary>
    public sealed partial class ByNameOrAliasPropertyComparer : IEqualityComparer<IProperty>
    {
        private readonly bool _ignoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByNameOrAliasPropertyComparer"/> class.
        /// </summary>
        /// <param name="ignoreCase">Ignore case for compare.</param>
        public ByNameOrAliasPropertyComparer(bool ignoreCase)
        {
            _ignoreCase = ignoreCase;
        }

        /// <inheritdoc/>
        public bool Equals(IProperty x, IProperty y)
        {
            return x.IsMatchesByNameOrAlias(y?.Name, _ignoreCase) || x.IsMatchesByNameOrAlias(y?.GetAlias(), _ignoreCase);
        }

        /// <inheritdoc/>
        public int GetHashCode(IProperty obj)
        {
            if (_ignoreCase)
                return obj.Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
            return obj.Name.GetHashCode();
        }
    }

    /// <summary>
    /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/>.
    /// </summary>
    public sealed partial class ByNameOrAliasPropertyComparer
    {
        /// <summary>
        /// Gets comparer that compares names ignoring the case.
        /// </summary>
        public static ByNameOrAliasPropertyComparer IgnoreCase { get; } = new ByNameOrAliasPropertyComparer(true);

        /// <summary>
        /// Gets comparer that compares using ordinal (binary) sort rules.
        /// </summary>
        public static ByNameOrAliasPropertyComparer Ordinal { get; } = new ByNameOrAliasPropertyComparer(false);
    }
}
