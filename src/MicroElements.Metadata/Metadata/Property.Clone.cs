// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata
{
    /// <content>
    /// Clone support for property.
    /// </content>
    public sealed partial class Property<T> : ICloneable<Property<T>>
    {
        /// <inheritdoc />
        public Property<T> Clone()
        {
            var clone = new Property<T>(Name, Description, Alias, DefaultValue, Examples, Calculator);
            this.CopyMetadataTo(clone);
            return clone;
        }
    }
}
