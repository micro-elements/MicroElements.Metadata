// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace MicroElements.Metadata
{
    public class MetadataProviderDebugView
    {
        public MetadataProviderDebugView(object value)
        {
            Value = value;
        }

        private object Value { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer Metadata => Value.AsMetadataProvider().GetInstanceMetadata(autoCreate: false);
    }
}
