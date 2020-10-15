// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparers.
    /// </summary>
    public static class PropertyComparer
    {
        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> DefaultEqualityComparer = new ByReferenceEqualityComparer();

        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> BeReferenceComparer = new ByReferenceEqualityComparer();

        /// <summary>
        /// Property comparer by <see cref="IProperty.Type"/> and <see cref="IProperty.Name"/>.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByTypeAndNameComparer = new ByTypeAndNameEqualityComparer();

        /// <summary>
        /// Property comparer by <see cref="IProperty.Type"/> and <see cref="IProperty.Name"/> ignore case.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByTypeAndNameIgnoreCaseComparer = new ByTypeAndNameEqualityComparer(StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Property comparer by <see cref="IProperty.Name"/> or <see cref="IProperty.Alias"/> ignore case.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByNameOrAliasIgnoreCase = ByNameOrAliasEqualityComparer.IgnoreCase;

        /// <summary>
        /// Property comparer by <see cref="IProperty.Name"/> or <see cref="IProperty.Alias"/> ignore case.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByNameOrAliasOrdinal = ByNameOrAliasEqualityComparer.Ordinal;

        /// <summary>
        /// Gets comparer ByNameOrAlias depending <paramref name="ignoreCase"/> flag.
        /// </summary>
        /// <param name="ignoreCase">Search ignore case.</param>
        /// <returns>Comparer instance.</returns>
        public static IEqualityComparer<IProperty> ByNameOrAlias(bool ignoreCase) => ignoreCase ? ByNameOrAliasIgnoreCase : ByNameOrAliasOrdinal;
    }
}
