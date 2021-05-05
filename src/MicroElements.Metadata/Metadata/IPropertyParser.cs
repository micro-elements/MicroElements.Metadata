// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent parser for property.
    /// </summary>
    [Obsolete("Will be replaced by IParserRule")]
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
    }

    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    [Obsolete("Will be replaced by IParserRule")]
    public interface IPropertyParser<T> : IPropertyParser
    {
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
