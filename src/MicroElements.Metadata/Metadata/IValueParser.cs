// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Text value parser.
    /// </summary>
    public interface IValueParser
    {
        /// <summary>
        /// Gets the target type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<object> ParseUntyped(string source);
    }

    /// <summary>
    /// Typed value parser.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IValueParser<T> : IValueParser
    {
        /// <summary>
        /// Parses string value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Parse result.</returns>
        Option<T> Parse(string source);
    }
}
