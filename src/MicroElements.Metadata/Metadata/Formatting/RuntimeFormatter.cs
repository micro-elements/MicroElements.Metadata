// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatter that gets formatter in runtime.
    /// </summary>
    public class RuntimeFormatter : IValueFormatter
    {
        private readonly Func<IValueFormatter> _formatterGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFormatter"/> class.
        /// </summary>
        /// <param name="formatterGetter">Function that returns formatter to use.</param>
        public RuntimeFormatter(Func<IValueFormatter> formatterGetter)
        {
            formatterGetter.AssertArgumentNotNull(nameof(formatterGetter));

            _formatterGetter = formatterGetter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return _formatterGetter().CanFormat(valueType);
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            return _formatterGetter().Format(value, valueType);
        }
    }
}
