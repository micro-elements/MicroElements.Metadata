// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Core
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Shows <see cref="IEnumerable{T}"/> as array in debug mode.
        /// </summary>
        public static IEnumerable<T> ToArrayDebug<T>(this IEnumerable<T> source)
        {
#if DEBUG
            return source.ToArray();
#else
            return source;
#endif
        }
    }
}
