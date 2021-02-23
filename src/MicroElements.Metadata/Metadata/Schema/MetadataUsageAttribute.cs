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
        SimpleSchema = 1 << 0,
        ObjectSchema = 1 << 1,
        RootSchema = 1 << 2,

        Property = 1 << 3,
        PropertyContainer = 1 << 4,

        AnySchema = SimpleSchema | ObjectSchema | RootSchema | Property,

        All = Property | PropertyContainer,
    }
}
