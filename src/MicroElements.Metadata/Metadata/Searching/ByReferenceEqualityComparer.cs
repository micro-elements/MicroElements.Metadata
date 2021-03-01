// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property comparer by reference equality.
    /// </summary>
    public sealed class ByReferenceEqualityComparer : IEqualityComparer<IProperty>
    {
        /// <inheritdoc/>
        public bool Equals(IProperty x, IProperty y) => ReferenceEquals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(IProperty obj) => obj.GetHashCode();
    }
}
