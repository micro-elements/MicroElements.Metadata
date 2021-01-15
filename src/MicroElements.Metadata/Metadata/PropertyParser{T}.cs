// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyParser<T> : IPropertyParser<T>
    {
        /// <inheritdoc />
        public Type TargetType => typeof(T);

        /// <inheritdoc />
        public string SourceName { get; }

        /// <inheritdoc />
        public IProperty TargetPropertyUntyped => TargetProperty;

        /// <inheritdoc />
        public IValueParser<T> ValueParser { get; }

        /// <inheritdoc />
        public IProperty<T> TargetProperty { get; private set; }

        /// <inheritdoc />
        public Func<T>? DefaultValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParser{T}"/> class.
        /// </summary>
        /// <param name="sourceName">Source name.</param>
        /// <param name="valueParser">Value parser.</param>
        /// <param name="targetProperty">Target property.</param>
        public PropertyParser(string sourceName, IValueParser<T> valueParser, IProperty<T>? targetProperty)
        {
            SourceName = sourceName ?? $"Undefined_{Guid.NewGuid()}";
            ValueParser = valueParser.AssertArgumentNotNull(nameof(valueParser));
            TargetProperty = targetProperty ?? new Property<T>($"UndefinedTarget_{SourceName}");
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{nameof(SourceName)}: {SourceName}, {nameof(TargetProperty)}: {TargetProperty}";

        /// <summary>
        /// Sets <see cref="TargetProperty"/> and returns this.
        /// </summary>
        /// <param name="targetProperty">Target property.</param>
        /// <returns>The same instance.</returns>
        public PropertyParser<T> Target(IProperty<T> targetProperty)
        {
            TargetProperty = targetProperty;
            return this;
        }

        /// <summary>
        /// Sets default value and returns this.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>The same instance.</returns>
        public PropertyParser<T> SetDefaultValue(T defaultValue)
        {
            DefaultValue = () => defaultValue;
            return this;
        }
    }
}
