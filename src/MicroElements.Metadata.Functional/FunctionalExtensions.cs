// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata.Functional
{
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Converts <see cref="Option{A}"/> to <see cref="ParseResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="option">Source option.</param>
        /// <returns><see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> ToParseResult<T>(this in Option<T> option)
        {
            if (option.IsSome)
                return ParseResult.Success((T)option);

            return ParseResult.Cache<T>.Failed;
        }
    }
}
