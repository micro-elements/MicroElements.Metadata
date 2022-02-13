// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property description.
    /// </summary>
    public interface IProperty : ISchema
    {
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IProperty<T> :
        ISchema<T>,
        IProperty,
        IHasAlias,
        IHas<IDefaultValue<T>>,
        IHas<IPropertyCalculator<T>>
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        IDefaultValue<T>? DefaultValue { get; }

        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        IPropertyCalculator<T>? Calculator { get; }

        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyList<T> Examples { get; }

        /// <inheritdoc />
        IDefaultValue<T>? IHas<IDefaultValue<T>>.Component => DefaultValue ?? this.GetMetadata<IDefaultValue<T>>();

        /// <inheritdoc />
        IPropertyCalculator<T>? IHas<IPropertyCalculator<T>>.Component => Calculator ?? this.GetMetadata<IPropertyCalculator<T>>();
    }
}
