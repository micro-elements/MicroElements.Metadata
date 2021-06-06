// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property set.
    /// </summary>
    public interface IPropertySet : IMetadataProvider
    {
        /// <summary>
        /// Gets properties enumeration.
        /// </summary>
        /// <returns><see cref="IProperty"/> enumeration.</returns>
        IEnumerable<IProperty> GetProperties();
    }
}
