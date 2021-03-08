// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public interface ISchemaId : IMetadata
    {
        /// <summary>
        /// Gets an alternative name for the object.
        /// </summary>
        string Id { get; }
    }
}
