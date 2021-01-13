// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class MetadataUsageAttribute : Attribute
    {
        public MetadataTargets ValidOn { get; set; }
    }

    [Flags]
    public enum MetadataTargets
    {
        Property = 1,
        PropertyContainer = 2,

        All = Property | PropertyContainer,
    }
}
