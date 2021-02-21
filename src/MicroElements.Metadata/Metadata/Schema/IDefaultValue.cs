// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Schema
{
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IDefaultValue : IMetadata
    {
        bool IsDefaultValueAllowed { get; }

        object? GetDefaultValue();
    }

    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IDefaultValue<T> : IDefaultValue
    {
        T DefaultValue { get; }

        /// <inheritdoc />
        object? IDefaultValue.GetDefaultValue() => DefaultValue;
    }
}
