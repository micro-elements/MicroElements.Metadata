// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object to metadata mapper and provides available property list.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    public interface IMetadataMapper<T> : IPropertySet, IModelMapper<T>
    {
    }
}
