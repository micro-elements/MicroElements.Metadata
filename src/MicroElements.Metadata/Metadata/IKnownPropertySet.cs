// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Describes type that knows its concrete <see cref="IPropertySet"/>.
    /// </summary>
    /// <typeparam name="TPropertySet">Property set type.</typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Ok.")]
    public interface IKnownPropertySet<TPropertySet>
        where TPropertySet : IPropertySet, new()
    {
    }
}
