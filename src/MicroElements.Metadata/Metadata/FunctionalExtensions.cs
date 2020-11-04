// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// FunctionalExtensions.
    /// </summary>
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Разложение списка списков в плоский список.
        /// </summary>
        /// <param name="listOfPropertyLists">Список списков.</param>
        /// <returns>Плоский список.</returns>
        [Obsolete("Use Flatten from MicroElements.Functional")]
        public static IEnumerable<IProperty> Flatten(this IEnumerable<IEnumerable<IProperty>> listOfPropertyLists) =>
            listOfPropertyLists.SelectMany(propertyList => propertyList);
    }
}
