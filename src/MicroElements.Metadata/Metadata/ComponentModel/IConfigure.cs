// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Base interface for all configure actions.
    /// </summary>
    public interface IConfigure
    {
        /// <summary>
        /// Gets the options type that can be configured.
        /// </summary>
        Type OptionType { get; }
    }

    /// <summary>
    /// Represent configure action for some type of Options.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public interface IConfigure<in TOptions> : IConfigure
    {
        /// <inheritdoc />
        Type IConfigure.OptionType => typeof(TOptions);

        /// <summary>
        /// Configures the provided options.
        /// </summary>
        /// <param name="options">Options to configure.</param>
        void Configure(TOptions options);
    }

    /// <summary>
    /// Configures options with action.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public sealed class Configure<TOptions> : IConfigure<TOptions>
    {
        private readonly Action<TOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configure{TOptions}"/> class.
        /// </summary>
        /// <param name="configure">Configure action.</param>
        public Configure(Action<TOptions> configure) => _configure = configure.AssertArgumentNotNull(nameof(configure));

        /// <inheritdoc />
        void IConfigure<TOptions>.Configure(TOptions options) => _configure(options);
    }

    /// <summary>
    /// Configures options with action list.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public sealed class ConfigureList<TOptions> : IConfigure<TOptions>
    {
        private readonly List<Action<TOptions>> _configure = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureList{TOptions}"/> class.
        /// </summary>
        /// <param name="configures">Configure actions.</param>
        public ConfigureList(params Action<TOptions>[]? configures)
        {
            if (configures != null)
                _configure.AddRange(configures);
        }

        /// <inheritdoc />
        void IConfigure<TOptions>.Configure(TOptions options)
        {
            foreach (Action<TOptions> configure in _configure)
            {
                configure(options);
            }
        }
    }

    /// <summary>
    /// Configurable builder.
    /// </summary>
    /// <typeparam name="TComposite">Composite type.</typeparam>
    /// <typeparam name="TOptions">Mutable configurable object.</typeparam>
    public interface IConfigurableBuilder<out TComposite, TOptions> :
        ICompositeBuilder<TComposite, TOptions>,
        ICompositeBuilder<TComposite, IConfigure<TOptions>>
    {
        /// <summary>
        /// Gets internal state copy as mutable object that will be used in configure actions.
        /// </summary>
        /// <returns>Mutable object that can be used in configure action.</returns>
        TOptions GetState();

        /// <inheritdoc />
        TComposite ICompositeBuilder<TComposite, IConfigure<TOptions>>.With(IConfigure<TOptions> configure)
        {
            TOptions options = GetState();
            configure.Configure(options);
            return With(options);
        }
    }

    /// <summary>
    /// ConfigurableBuilder Extensions.
    /// </summary>
    public static class ConfigurableBuilderExtensions
    {
        /// <summary>
        /// Configures composite with provided configure action.
        /// </summary>
        /// <typeparam name="TComposite">Composite type.</typeparam>
        /// <typeparam name="TOptions">Options type.</typeparam>
        /// <param name="builder">Configurable object.</param>
        /// <param name="configure">Configure action.</param>
        /// <returns>Configured object.</returns>
        public static TComposite Configure<TComposite, TOptions>(
            this IConfigurableBuilder<TComposite, TOptions> builder,
            Action<TOptions> configure)
        {
            return builder.With(new Configure<TOptions>(configure));
        }

        /// <summary>
        /// Configures composite with provided configure action.
        /// </summary>
        /// <typeparam name="TComposite">Composite type.</typeparam>
        /// <typeparam name="TOptions">Options type.</typeparam>
        /// <param name="builder">Configurable object.</param>
        /// <param name="configure">Configure action.</param>
        /// <returns>Configured object.</returns>
        public static TComposite Configure<TComposite, TOptions>(
            this ICompositeBuilder<TComposite, IConfigure<TOptions>> builder,
            Action<TOptions> configure)
        {
            return builder.With(new Configure<TOptions>(configure));
        }
    }
}
