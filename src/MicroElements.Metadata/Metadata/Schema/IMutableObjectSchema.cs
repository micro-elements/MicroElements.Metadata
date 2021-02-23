// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema for <see cref="IPropertyContainer"/>.
    /// Contains properties, constraints and validation rules.
    /// </summary>
    public interface IMutableObjectSchema : IObjectSchema
    {
        /// <summary>
        /// Adds property to the schema.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Property that was added.</returns>
        IProperty AddProperty(IProperty property);
    }
}
