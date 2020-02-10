// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Value mapper from <see cref="SourceType"/> to <see cref="TargetType"/>.
    /// </summary>
    public interface IValueMapper
    {
        /// <summary>
        /// Gets source type.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets target type.
        /// </summary>
        Type TargetType { get; }
    }

    /// <summary>
    /// Value mapper from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    public interface IValueMapper<in TSource, TTarget> : IValueMapper
    {
        /// <summary>
        /// Convert source value to target type.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <returns>Map result.</returns>
        Option<TTarget> Map(TSource source);
    }
}
