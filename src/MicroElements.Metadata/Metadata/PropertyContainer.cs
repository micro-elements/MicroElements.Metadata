// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Base class for <see cref="IPropertyContainer"/> objects.
    /// </summary>
    public abstract class PropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Gets an empty property container.
        /// </summary>
        public static IPropertyContainer Empty => EmptyPropertyContainer.Instance;

        protected PropertyList PropertyList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        /// <param name="values">Property values.</param>
        protected PropertyContainer(IEnumerable<IPropertyValue> values = null)
        {
            PropertyList = new PropertyList(values);
        }

        /// <inheritdoc />
        public IPropertyContainer ParentSource => PropertyList.ParentSource;

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => PropertyList;

        /// <inheritdoc />
        public T GetValue<T>(IProperty<T> property) => PropertyList.GetValue(property);

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property) => PropertyList.GetValueUntyped(property);

        /// <inheritdoc />
        public override string ToString() => PropertyList.ToString();

        /// <summary>
        /// Sets value for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value to store.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public IPropertyValue<T> SetValue<T>(IProperty<T> property, T value) => PropertyList.SetValue(property, value);

        public void SetValueIfNotSet<T>(IProperty<T> property, T value)
        {
            if (!PropertyList.ContainsPropertyByNameOrAlias(property.Name))
                PropertyList.SetValue(property, value);
        }

        public void SetParentSource(IPropertyContainer propertyContainer)
        {
            PropertyList = PropertyList.SetParentPropertySource(propertyContainer);
        }
    }
}
