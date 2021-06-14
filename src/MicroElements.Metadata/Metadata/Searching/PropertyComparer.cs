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
        public static IEqualityComparer<IProperty> DefaultEqualityComparer { get; } = new ByTypeAndNamePropertyComparer();

        /// <summary>
        /// DefaultMetadataComparer.
        /// Compares by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> DefaultMetadataComparer { get; } = new ByTypeAndNamePropertyComparer(StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static IEqualityComparer<IProperty> ByReferenceComparer { get; } = new ByReferencePropertyComparer();

        /// <summary>
        /// Property comparer by Type and Name.
        /// </summary>
        public static IEqualityComparer<IProperty> ByTypeAndNameComparer { get; } = new ByTypeAndNamePropertyComparer();

        /// <summary>
        /// Property comparer by Type and Name ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByTypeAndNameIgnoreCaseComparer { get; } = new ByTypeAndNamePropertyComparer(typeNameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: false);

        /// <summary>
        /// Property comparer by Type and Name ignoring name case and ignoring <see cref="Nullable{T}"/> wrapper.
        /// </summary>
        public static IEqualityComparer<IProperty> ByTypeAndNameIgnoreCaseIgnoreNullability { get; } = new ByTypeAndNamePropertyComparer(typeNameComparison: StringComparison.OrdinalIgnoreCase, ignoreTypeNullability: true);

        /// <summary>
        /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/> ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByNameOrAliasIgnoreCase { get; } = ByNameOrAliasPropertyComparer.IgnoreCase;

        /// <summary>
        /// Property comparer by <see cref="ISchema.Name"/> or <see cref="IHasAlias.Alias"/> ignore case.
        /// </summary>
        public static IEqualityComparer<IProperty> ByNameOrAliasOrdinal { get; } = ByNameOrAliasPropertyComparer.Ordinal;

        /// <summary>
        /// Gets comparer ByNameOrAlias depending <paramref name="ignoreCase"/> flag.
        /// </summary>
        /// <param name="ignoreCase">Search ignore case.</param>
        /// <returns>Comparer instance.</returns>
        public static IEqualityComparer<IProperty> ByNameOrAlias(bool ignoreCase) => ignoreCase ? ByNameOrAliasIgnoreCase : ByNameOrAliasOrdinal;
    }
}
