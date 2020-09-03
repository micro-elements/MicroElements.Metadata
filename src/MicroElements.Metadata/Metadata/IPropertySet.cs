// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property set.
    /// Has default implementation of <see cref="IEnumerable{T}"/> of <see cref="IProperty"/>.
    /// </summary>
    public interface IPropertySet : IMetadataProvider, IEnumerable<IProperty>
    {
        /// <summary>
        /// Gets properties enumeration.
        /// </summary>
        /// <returns><see cref="IProperty"/> enumeration.</returns>
        IEnumerable<IProperty> GetProperties();

        #region IEnumerable<IProperty> implementation

        private IEnumerator<IProperty> GetEnumeratorInternal() => GetProperties().GetEnumerator();

        IEnumerator<IProperty> IEnumerable<IProperty>.GetEnumerator() => GetEnumeratorInternal();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

        #endregion
    }
}
