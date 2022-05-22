// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.CompositeBuilder;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Builds recursive formatters.
    /// </summary>
    public class FormatterBuilder
    {
        private readonly List<IValueFormatter> _formatters = new List<IValueFormatter>();
        private readonly List<IConfigure> _configure = new List<IConfigure>();

        /// <summary>
        /// Gets the runtime formatter with reference to <see cref="FullRecursiveFormatter"/>.
        /// </summary>
        public IValueFormatter RuntimeFormatter { get; }

        /// <summary>
        /// Gets result formatter.
        /// </summary>
        public IValueFormatter FullRecursiveFormatter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterBuilder"/> class.
        /// </summary>
        public FormatterBuilder()
        {
            RuntimeFormatter = new RuntimeFormatter<FormatterBuilder>(this, builder => builder.FullRecursiveFormatter);
        }

        public static FormatterBuilder Create() => new FormatterBuilder();



        public FormatterBuilder AddStandardFormatters(
            bool collectionFormatter = true,
            bool keyValuePairFormatter = true,
            bool valueTuplePairFormatter = true)
        {
            if (collectionFormatter)
                _formatters.Add(new CollectionFormatter(RuntimeFormatter));

            if (keyValuePairFormatter)
                _formatters.Add(new KeyValuePairFormatter(RuntimeFormatter));

            if (valueTuplePairFormatter)
                _formatters.Add(new ValueTuplePairFormatter(RuntimeFormatter));

            _formatters.Add(DefaultToStringFormatter.Instance);
            return this;
        }

        public FormatterBuilder WithFormatters(IValueFormatterProvider formatterProvider)
        {
            formatterProvider.AssertArgumentNotNull();
            _formatters.AddRange(formatterProvider.GetFormatters());
            return this;
        }

        public FormatterBuilder WithFormatters(Func<IValueFormatter, IValueFormatterProvider> factory)
        {
            factory.AssertArgumentNotNull();
            _formatters.AddRange(factory(RuntimeFormatter).GetFormatters());
            return this;
        }

        public FormatterBuilder WithFormatter(IValueFormatter formatter)
        {
            _formatters.Add(formatter);
            return this;
        }

        public FormatterBuilder WithFormatter(Func<IValueFormatter, IValueFormatter> factory)
        {
            _formatters.Add(factory(RuntimeFormatter));
            return this;
        }

        public FormatterBuilder ConfigureFormatter<TOptions>(Action<TOptions> configure)
        {
            configure.AssertArgumentNotNull(nameof(configure));

            _configure.Add(new Configure<TOptions>(configure));
            return this;
        }

        public IValueFormatter Build()
        {
            if (_configure.Count > 0)
            {
                var configureForFormatter = _formatters
                    .ToDictionary(f => f, f => new Lazy<List<IConfigure>>(() => new List<IConfigure>()));

                foreach (IValueFormatter valueFormatter in _formatters)
                {
                    foreach (IConfigure configure in _configure)
                    {
                        Type configureType = typeof(IConfigure<>).MakeGenericType(configure.OptionType);
                        Type builderType = typeof(ICompositeBuilder<>).MakeGenericType(configureType);

                        if (valueFormatter.IsAssignableTo(builderType))
                        {
                            configureForFormatter[valueFormatter].Value.Add(configure);
                        }
                    }
                }

                for (var i = 0; i < _formatters.Count; i++)
                {
                    IValueFormatter valueFormatter = _formatters[i];
                    if (configureForFormatter[valueFormatter].IsValueCreated)
                    {
                        foreach (IConfigure configure in configureForFormatter[valueFormatter].Value)
                        {
                            Type configureType = typeof(IConfigure<>).MakeGenericType(configure.OptionType);
                            Type builderType = typeof(ICompositeBuilder<>).MakeGenericType(configureType);
                            MethodInfo builderWith = builderType.GetMethod(nameof(ICompositeBuilder<object>.With), types: new []{ configureType });
                            object result = builderWith.Invoke(valueFormatter, new object[] { configure });
                            valueFormatter = (IValueFormatter)result;
                        }

                        _formatters[i] = valueFormatter;
                    }
                }
            }

            FullRecursiveFormatter = new CompositeFormatter(_formatters);
            return FullRecursiveFormatter;
        }
    }
}
