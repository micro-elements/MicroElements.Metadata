// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// <see cref="IValueFormatter"/> that gets the formatter in runtime.
    /// It's used to create recursive formatters.
    /// </summary>
    public class RuntimeFormatter : IValueFormatter
    {
        private readonly Func<IValueFormatter> _formatterGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFormatter"/> class.
        /// </summary>
        /// <param name="formatterGetter">Function that returns formatter.</param>
        public RuntimeFormatter(Func<IValueFormatter> formatterGetter)
        {
            _formatterGetter = formatterGetter.AssertArgumentNotNull();
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return _formatterGetter().CanFormat(valueType);
        }

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            return _formatterGetter().Format(value, valueType);
        }
    }

    /// <summary>
    /// <see cref="IValueFormatter"/> that gets the formatter in runtime.
    /// It's used to create recursive formatters.
    /// </summary>
    /// <typeparam name="T">Type that contains formatter instance.</typeparam>
    public class RuntimeFormatter<T> : IValueFormatter
    {
        private readonly T _host;
        private readonly Func<T, IValueFormatter> _formatterGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFormatter{T}"/> class.
        /// </summary>
        /// <param name="host">The instance that contains formatter.</param>
        /// <param name="formatterGetter">Function that gets formatter in runtime.</param>
        public RuntimeFormatter(T host, Func<T, IValueFormatter> formatterGetter)
        {
            _host = host;
            _formatterGetter = formatterGetter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return _formatterGetter(_host).CanFormat(valueType);
        }

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            return _formatterGetter(_host).Format(value, valueType);
        }
    }
}
