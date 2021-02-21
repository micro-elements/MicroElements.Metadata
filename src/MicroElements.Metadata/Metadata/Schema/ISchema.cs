// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema for <see cref="IPropertyContainer"/>.
    /// Contains properties, constraints and validation rules.
    /// </summary>
    public interface ISchema : IPropertySet
    {
        //string Id { get; }// add Description etc

        /// <summary>
        /// Adds property to the schema.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Property that was added.</returns>
        IProperty AddProperty(IProperty property);

        /// <summary>
        /// Gets property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
        IProperty? GetProperty(string name);
    }

    public interface ISchemaNew : IMetadataProvider, IMetadata
    {
        IPropertyContainer ISchemaDescription { get; }

        IPropertySet Properties { get; }

        object[] Items { get; }

        ICombinator Combinator { get; }
    }
}
