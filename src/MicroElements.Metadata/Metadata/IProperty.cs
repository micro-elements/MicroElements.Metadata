// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property description.
    /// Every object consist of many properties and we should map properties from different sources to one common model.
    /// </summary>
    public interface IProperty : IMetadataProvider
    {
        /// <summary>
        /// Gets property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets property value type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets property description.
        /// </summary>
        LocalizableString? Description { get; }

        /// <summary>
        /// Gets alternative name for property.
        /// </summary>
        string? Alias { get; }
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IProperty<T> : IProperty
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        Func<T> DefaultValue { get; }

        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        IPropertyCalculator<T>? Calculator { get; }

        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyList<T> Examples { get; }
    }
}
