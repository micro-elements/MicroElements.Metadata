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

        /// <summary>
        /// Gets property enumerator.
        /// </summary>
        /// <returns>Enumerator instance.</returns>
        private IEnumerator<IProperty> GetEnumeratorInternal() => GetProperties().GetEnumerator();

        /// <inheritdoc cref="GetEnumeratorInternal"/>
        IEnumerator<IProperty> IEnumerable<IProperty>.GetEnumerator() => GetEnumeratorInternal();

        /// <inheritdoc cref="GetEnumeratorInternal"/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

        #endregion
    }
}
