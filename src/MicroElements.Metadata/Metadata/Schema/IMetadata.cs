// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Marker interface for metadata types.
    /// </summary>
    public interface IMetadata
    {
    }

    /// <summary>
    /// Marks metadata as ReadOnly.
    /// - ReadOnly objects can have methods for object modification but modifications are prohibited.
    /// - ReadOnly objects can be cached and shared with cautions (if object can not be modified outside).
    /// </summary>
    public interface IReadOnly
    {
    }

    /// <summary>
    /// Marks metadata as Immutable.
    /// - Immutable objects has no methods for object modification.
    /// - Immutable objects can be cached and shared.
    /// </summary>
    public interface IImmutable : IReadOnly
    {
    }
}
