// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Static property set gets properties from static fields and properties so you should not to implement <see cref="IPropertySet.GetProperties"/>.
    /// </summary>
    public interface IStaticPropertySet : IPropertySet
    {
        /// <inheritdoc />
        IEnumerable<IProperty> IPropertySet.GetProperties() => GetType().GetStaticProperties();
    }

    /// <summary>
    /// Static property set gets properties from static fields and properties so you should not to implement <see cref="IPropertySet.GetProperties"/>.
    /// </summary>
    public class StaticPropertySet : IStaticPropertySet
    {
        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties() => GetType().GetStaticProperties();
    }
}
