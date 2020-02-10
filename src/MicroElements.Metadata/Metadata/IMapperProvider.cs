// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property mappers.
    /// </summary>
    public interface IMapperProvider
    {
        /// <summary>
        /// Gets property mappers.
        /// </summary>
        IEnumerable<IPropertyMapper> Mappers { get; }
    }
}
