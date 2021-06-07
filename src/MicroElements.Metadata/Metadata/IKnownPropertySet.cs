// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Describes type that knows its concrete <see cref="IPropertySet"/>.
    /// </summary>
    public interface IKnownPropertySet
    {
        /// <summary>
        /// Gets known schema (or property set) type.
        /// </summary>
        Type SchemaType { get; }
    }

    /// <summary>
    /// Describes type that knows its concrete <see cref="IPropertySet"/>.
    /// </summary>
    /// <typeparam name="TPropertySet">Property set type.</typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Ok.")]
    public interface IKnownPropertySet<TPropertySet> : IKnownPropertySet
        where TPropertySet : IPropertySet, new()
    {
        /// <inheritdoc />
        Type IKnownPropertySet.SchemaType => typeof(TPropertySet);
    }
}
