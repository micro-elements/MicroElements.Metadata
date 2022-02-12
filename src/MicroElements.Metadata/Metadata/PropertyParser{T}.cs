// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public sealed class PropertyParser<T> :
        IPropertyParser<T>,
        IPropertyParserCondition,
        IPropertyParserNotifier
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
        public IDefaultValue<T>? DefaultValue { get; private set; }

        /// <inheritdoc />
        public IDefaultValue<string>? DefaultSourceValue { get; private set; }

        /// <inheritdoc />
        public Func<PropertyParserContext, bool>? ExcludeCondition { get; private set; }

        /// <inheritdoc />
        public Func<PropertyParserContext, bool>? IncludeCondition { get; private set; }

        /// <inheritdoc />
        public Action<PropertyParserContext, ParseResult<IPropertyValue>>? PropertyParsed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParser{T}"/> class.
        /// </summary>
        /// <param name="sourceName">Source name.</param>
        /// <param name="valueParser">Value parser.</param>
        /// <param name="targetProperty">Target property.</param>
        public PropertyParser(string? sourceName, IValueParser<T> valueParser, IProperty<T>? targetProperty)
        {
            SourceName = sourceName ?? $"Undefined_{Guid.NewGuid()}";
            ValueParser = valueParser.AssertArgumentNotNull(nameof(valueParser));
            TargetProperty = targetProperty ?? new Property<T>($"UndefinedTarget_{SourceName}");
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(SourceName)}: {SourceName}, {nameof(TargetProperty)}: {TargetProperty}";

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
            DefaultValue = new DefaultValue<T>(defaultValue);
            return this;
        }

        /// <summary>
        /// Sets default value that can used if source value is absent or null.
        /// </summary>
        /// <param name="defaultSourceValue">Default source value.</param>
        /// <returns>The same instance.</returns>
        public PropertyParser<T> SetDefaultSourceValue(string defaultSourceValue)
        {
            DefaultSourceValue = new DefaultValue<string>(defaultSourceValue);
            return this;
        }

        public PropertyParser<T> SkipNull()
        {
            ExcludeCondition = context => context.IsValueNull();
            return this;
        }

        public PropertyParser<T> SkipAbsent()
        {
            ExcludeCondition = context => context.IsValueAbsent();
            return this;
        }

        public PropertyParser<T> SkipAbsentAndNull()
        {
            ExcludeCondition = context => context.IsValueAbsentOrNull();
            return this;
        }

        public PropertyParser<T> SetNotifier(IPropertyParserNotifier notifier)
        {
            PropertyParsed = notifier.PropertyParsed;
            return this;
        }

        public PropertyParser<T> SetNotifier(Action<PropertyParserContext, ParseResult<IPropertyValue>>? propertyParsed)
        {
            PropertyParsed = propertyParsed;
            return this;
        }

        public PropertyParser<T> Condition(Func<PropertyParserContext, bool>? condition)
        {
            IncludeCondition = condition;
            return this;
        }

        public PropertyParser<T> Discriminator(Func<IPropertyValue, bool> condition)
        {
            return Condition(context =>
            {
                if (context.ParseContext.ParserProvider is IParserProviderWithDiscriminator { Discriminator: { } discriminator })
                {
                    var propertyValue = context.PropertyContainer.GetPropertyValueUntyped(discriminator);
                    if (propertyValue != null)
                        return condition(propertyValue);
                }

                return true;
            });
        }

        public PropertyParser<T> Discriminator(object discriminatorValue)
        {
            return Discriminator(pv => Equals(pv.ValueUntypedOrNull(), discriminatorValue));
        }
    }
}
