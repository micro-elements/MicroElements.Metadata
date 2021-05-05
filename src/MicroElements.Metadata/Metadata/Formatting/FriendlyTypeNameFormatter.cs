// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Validation;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// 
    /// </summary>
    public class FriendlyTypeNameFormatter : IValueFormatter<Type>
    {
        /// <summary>
        /// Gets global static instance for reuse.
        /// </summary>
        public static IValueFormatter<Type> Instance { get; } = new FriendlyTypeNameFormatter();

        /// <inheritdoc />
        public string Format(Type value)
        {
            return value.GetFriendlyName();
        }
    }
}
