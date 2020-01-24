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
        protected PropertyList PropertyList { get; private set; }

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

        public void SetValue<T>(IProperty<T> property, T value) => PropertyList.SetValue(property, value);

        public void SetValueIfNotSet<T>(IProperty<T> property, T value)
        {
            if (!PropertyList.HasProperty(property))
                PropertyList.SetValue(property, value);
        }

        /// <inheritdoc />
        public override string ToString() => PropertyList.ToString();

        public void SetParentSource(IPropertyContainer propertyContainer)
        {
            PropertyList = PropertyList.SetParentPropertySource(propertyContainer);
        }
    }
}
