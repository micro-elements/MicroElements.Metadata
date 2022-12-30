// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Diagnostics;

namespace MicroElements.Metadata.Parsing
{
    public class ParserResultUntyped : IParseResult
    {
        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public bool IsSuccess { get; }

        /// <inheritdoc />
        public object? ValueUntyped { get; }

        /// <inheritdoc />
        public Message? Error { get; }

        public ParserResultUntyped(Type type, bool isSuccess, object? valueUntyped, Message? error)
        {
            Type = type;
            IsSuccess = isSuccess;
            ValueUntyped = valueUntyped;
            Error = error;
        }
    }
}
