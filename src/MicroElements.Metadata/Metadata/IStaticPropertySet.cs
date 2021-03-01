// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;

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

    /// <summary>
    /// Static schema gets properties from static fields and properties.
    /// </summary>
    public interface IStaticSchema : IObjectSchemaProvider
    {
        /// <inheritdoc />
        IObjectSchema IObjectSchemaProvider.GetObjectSchema()
        {
            var properties = GetType().GetStaticProperties();
            return new MutableObjectSchema(name: GetType().Name, properties: properties);
        }
    }

    /// <summary>
    /// Static schema gets properties from static fields and properties.
    /// </summary>
    public class StaticSchema : IObjectSchemaProvider
    {
        /// <inheritdoc />
        public IObjectSchema GetObjectSchema()
        {
            var properties = GetType().GetStaticProperties();
            return new MutableObjectSchema(name: GetType().Name, properties: properties);
        }
    }
}
