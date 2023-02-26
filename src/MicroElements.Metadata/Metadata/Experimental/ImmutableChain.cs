// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MicroElements.Text.StringFormatter;

#pragma warning disable CS1591
#pragma warning disable SA1401: Fields should be private
#pragma warning disable SA1611: Element parameters should be documented
#pragma warning disable SA1615: Element return value should be documented
#pragma warning disable SA1618: Generic type parameters should be documented
#pragma warning disable SA1600: Elements should be documented

namespace MicroElements.Metadata.Experimental
{
    /// <summary>
    /// Immutable object chain.
    /// </summary>
    /// <typeparam name="T">The type of elements in the chain.</typeparam>
    public class ImmutableChain<T> : IReadOnlyList<T>
    {
        public static readonly ImmutableChain<T> Empty = new(value: default, left: null, right: null, isValue: false);

        internal readonly T? Value;
        internal readonly ImmutableChain<T>? Left;
        internal readonly ImmutableChain<T>? Right;

        // Allows to cache values in a list for cheap iterations.
        private readonly Lazy<IReadOnlyList<T>> _lazyValues;

        internal ImmutableChain(T? value = default, ImmutableChain<T>? left = null, ImmutableChain<T>? right = null, bool isValue = true)
        {
            if (isValue && left == null && right == null)
                (Value, Left, Right) = (value, this, null);
            else
                (Value, Left, Right) = (value, left, right);

            _lazyValues = new(this.ToArray);
        }

        /// <summary>
        /// Gets cached values as list. Enumeration is expensive in hot paths.
        /// </summary>
        public IReadOnlyList<T> Values => _lazyValues.Value;

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            if (Left != null)
            {
                if (Left == this)
                {
                    yield return Value!;
                }
                else
                {
                    foreach (var value in Left)
                    {
                        yield return value;
                    }
                }
            }

            if (Right != null)
            {
                foreach (var value in Right)
                {
                    yield return value;
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => Values.Count;

        /// <inheritdoc />
        public T this[int index] => Values[index];

        /// <inheritdoc />
        public override string ToString() => this.FormatAsTuple();
    }

    /// <summary>
    /// ImmutableChain extensions.
    /// </summary>
    public static class ImmutableChain
    {
        /// <summary> Gets value indicating that chain is null or empty. </summary>
        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ImmutableChain<T>? chain)
            => chain is null || (chain.Left == null && chain.Right == null);

        /// <summary> Creates chain with provided elements. Can create empty collection. </summary>
        public static ImmutableChain<T> Create<T>(params T[]? values) =>
            values == null || values.Length == 0
                ? ImmutableChain<T>.Empty
                : values.Aggregate(ImmutableChain<T>.Empty, (current, value) => current.Append(value));

        /// <summary>
        /// Appends <paramref name="value"/> to the end of collection.
        /// </summary>
        /// <param name="chain">Prev collection instance.</param>
        /// <param name="value">Value to append.</param>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>New collection instance.</returns>
        public static ImmutableChain<T> Append<T>(this ImmutableChain<T>? chain, T value)
            => chain.Append(new ImmutableChain<T>(value));

        /// <summary>
        /// Appends other chain to the source.
        /// </summary>
        /// <param name="chain">The source collection.</param>
        /// <param name="other">Values to append.</param>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>New collection instance.</returns>
        public static ImmutableChain<T> Append<T>(this ImmutableChain<T>? chain, ImmutableChain<T> other)
            => chain.IsNullOrEmpty() ? other : new ImmutableChain<T>(left: chain, right: other);

        /// <summary>
        /// Prepends collection with provided value.
        /// </summary>
        /// <param name="chain">Prev collection instance.</param>
        /// <param name="value">Value to prepend.</param>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>New collection instance.</returns>
        public static ImmutableChain<T> Prepend<T>(this ImmutableChain<T>? chain, T value)
            => chain.IsNullOrEmpty() ? new ImmutableChain<T>(value) : new ImmutableChain<T>(value).Append(chain);
    }
}
