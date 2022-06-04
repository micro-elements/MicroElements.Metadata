// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.ComponentModel
{
    public interface IConfigure
    {
        Type OptionType { get; }
    }

    public interface IConfigure<in TOptions> : IConfigure
    {
        /// <inheritdoc />
        Type IConfigure.OptionType => typeof(TOptions);

        void Configure(TOptions options);
    }

    class Configure<TOptions> : IConfigure<TOptions>
    {
        private readonly Action<TOptions> _configure;

        public Configure(Action<TOptions> configure) => _configure = configure;

        /// <inheritdoc />
        void IConfigure<TOptions>.Configure(TOptions options) => _configure(options);
    }
}
