// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        public T GetValue<T>(IProperty<T> property) => property.DefaultValue();

        /// <inheritdoc />
        public object GetValueUntyped(IProperty property) => null;
    }
}
