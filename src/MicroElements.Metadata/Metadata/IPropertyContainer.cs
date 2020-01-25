// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public interface IPropertyContainer
    {
        /// <summary>
        /// Gets parent property source.
        /// </summary>
        IPropertyContainer ParentSource { get; }

        /// <summary>
        /// Gets properties with values.
        /// </summary>
        IReadOnlyList<IPropertyValue> Properties { get; }

        /// <summary>
        /// Gets the value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <returns>The value for property.</returns>
        T GetValue<T>(IProperty<T> property);

        /// <summary>
        /// Gets untyped value for property.
        /// </summary>
        /// <param name="property">Property to find.</param>
        /// <returns>The value for property.</returns>
        object GetValueUntyped(IProperty property);
    }

    public interface IMutablePropertyContainer
    {
        IPropertyValue<T> SetValue<T>(IProperty<T> property, T value);
    }
}
