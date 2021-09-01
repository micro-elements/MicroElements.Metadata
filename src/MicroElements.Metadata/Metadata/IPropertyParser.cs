// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent parser for property.
    /// </summary>
    public interface IPropertyParser
    {
        /// <summary>
        /// Gets source name.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Gets target property type.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets target property.
        /// </summary>
        IProperty TargetPropertyUntyped { get; }

        /// <summary>
        /// Gets default value that can used if source value is absent or null.
        /// </summary>
        IDefaultValue<string>? DefaultSourceValue { get; }

        /// <summary>
        /// Gets default property value (untyped).
        /// </summary>
        IDefaultValue? DefaultValueUntyped { get; }
    }

    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyParser<T> : IPropertyParser
    {
        /// <inheritdoc />
        IDefaultValue? IPropertyParser.DefaultValueUntyped => DefaultValue;

        /// <summary>
        /// Gets value parser.
        /// </summary>
        IValueParser<T> ValueParser { get; }

        /// <summary>
        /// Gets target property.
        /// </summary>
        IProperty<T> TargetProperty { get; }

        /// <summary>
        /// Gets default property value.
        /// </summary>
        IDefaultValue<T>? DefaultValue { get; }
    }
}
