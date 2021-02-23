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
    /// Marks immutable metadata.
    /// Immutable objects can be cached and shared.
    /// </summary>
    public interface IImmutable
    {
    }

    public interface IReadOnly
    {
    }
}
