// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Empty property container singleton.
    /// </summary>
    public class EmptyPropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Empty property container singleton instance.
        /// </summary>
        public static readonly EmptyPropertyContainer Instance = new EmptyPropertyContainer();

        private EmptyPropertyContainer()
        {
        }

        /// <inheritdoc />
        public IPropertyContainer ParentSource => Instance;

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Properties => Array.Empty<IPropertyValue>();

        /// <inheritdoc />
        public IPropertyValue<T> GetPropertyValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default) => new PropertyValue<T>(property, default, ValueSource.NotDefined);

        /// <inheritdoc />
        public IPropertyValue GetPropertyValueUntyped(IProperty property, PropertySearch propertySearch = PropertySearch.Default) => null;

        /// <inheritdoc />
        public T GetValue<T>(IProperty<T> property, PropertySearch propertySearch = PropertySearch.Default) => property.DefaultValue();

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property) => null;

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator()
        {
            yield break;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => 0;

        /// <inheritdoc />
        public IPropertyValue this[int index] => null;
    }
}
