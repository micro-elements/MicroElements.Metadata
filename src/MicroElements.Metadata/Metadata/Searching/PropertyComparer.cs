// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "Ok.")]
    public static class PropertyComparer
    {
        /// <summary>
        /// DefaultEqualityComparer.
        /// Compares by Type and Name.
        /// </summary>
        public static IEqualityComparer<IProperty> DefaultEqualityComparer { get; } = new ByTypeAndNameEqualityComparer();

        /// <summary>
        /// DefaultMetadataComparer.
        /// Compares by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> DefaultMetadataComparer { get; } = new ByTypeAndNameEqualityComparer(StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static IEqualityComparer<IProperty> ByReferenceComparer { get; } = new ByReferenceEqualityComparer();

        /// <summary>
        /// Property comparer by Type and Name.
        /// </summary>
        public static IEqualityComparer<IProperty> ByTypeAndNameComparer { get; } = new ByTypeAndNameEqualityComparer();

        /// <summary>
        /// Property comparer by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByTypeAndNameIgnoreCaseComparer { get; } = new ByTypeAndNameEqualityComparer(StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/> ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByNameOrAliasIgnoreCase { get; } = ByNameOrAliasEqualityComparer.IgnoreCase;

        /// <summary>
        /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/> ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByNameOrAliasOrdinal { get; } = ByNameOrAliasEqualityComparer.Ordinal;

        /// <summary>
        /// Gets comparer ByNameOrAlias depending <paramref name="ignoreCase"/> flag.
        /// </summary>
        /// <param name="ignoreCase">Search ignore case.</param>
        /// <returns>Comparer instance.</returns>
        public static IEqualityComparer<IProperty> ByNameOrAlias(bool ignoreCase) => ignoreCase ? ByNameOrAliasIgnoreCase : ByNameOrAliasOrdinal;
    }
}
