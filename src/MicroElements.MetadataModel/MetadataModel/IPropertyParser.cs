// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using static MicroElements.Functional.Prelude;

namespace MicroElements.MetadataModel
{
    /// <summary>
    /// Represent parser for property.
    /// </summary>
    public interface IPropertyParser
    {
        /// <summary>
        /// Gets property type.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets source name.
        /// </summary>
        string SourceName { get; }
    }

    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyParser<T> : IPropertyParser
    {
        /// <summary>
        /// Gets value parser.
        /// </summary>
        IValueParser<T> ValueParser { get; }

        /// <summary>
        /// Gets target property.
        /// </summary>
        IProperty<T> TargetProperty { get; }

        /// <summary>
        /// Gets default property value.
        /// </summary>
        Func<T> DefaultValue { get; }
    }

    /// <summary>
    /// Generic property parser.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyParser<T> : IPropertyParser<T>
    {
        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public string SourceName { get; private set; }

        /// <inheritdoc />
        public IValueParser<T> ValueParser { get; private set; }

        /// <inheritdoc />
        public IProperty<T> TargetProperty { get; private set; }

        /// <inheritdoc />
        public Func<T> DefaultValue { get; private set; }

        /// <inheritdoc />
        public PropertyParser(string sourceName, IValueParser<T> valueParser, IProperty<T> targetProperty)
        {
            SourceName = sourceName ?? "UNDEFINED";
            ValueParser = valueParser;
            TargetProperty = targetProperty;
        }

        public PropertyParser<T> Target(IProperty<T> targetProperty)
        {
            TargetProperty = targetProperty;
            return this;
        }

        public PropertyParser<T> SetDefaultValue(T defaultValue)
        {
            DefaultValue = () => defaultValue;
            return this;
        }

        public PropertyParser<T> Parse(string sourceName, IValueParser<T> valueParser)
        {
            SourceName = sourceName;
            ValueParser = valueParser;
            return this;
        }

        [Obsolete("В этом случае не меняется уже добавленная запись")]
        public PropertyParser<T2> Parse<T2>(Func<string, T2> parseFunc)
        {
            return new PropertyParser<T2>(SourceName, new ValueParser<T2>(parseFunc), null);
        }

        [Obsolete("В этом случае не меняется уже добавленная запись")]
        public PropertyParser<T2> Parse<T2>(IValueParser<T2> valueParser)
        {
            return new PropertyParser<T2>(SourceName, valueParser, null);
        }
    }

    public static class PropertyParserExtensions
    {

        public static PropertyParser<string> ParseAsString(this PropertyParser<string> propertyParser, string sourceName)
        {
            return propertyParser.Parse(sourceName, StringParser.Instance);
        }

        public static Option<IPropertyValue> GetDefaultValueUntyped(this IPropertyParser propertyParser) =>
            GetDefaultValueFunc(propertyParser.PropertyType).Invoke(propertyParser);

        public static readonly Func<Type, Func<IPropertyParser, Option<IPropertyValue>>> GetDefaultValueFunc =
            CodeCompiler.CreateCompiledFunc<IPropertyParser, Option<IPropertyValue>>(GetDefaultValue<CodeCompiler.GenericType>);

        public static Option<IPropertyValue> GetDefaultValue<T>(this IPropertyParser propertyParserUntyped)
        {
            IPropertyParser<T> propertyParser = (IPropertyParser<T>)propertyParserUntyped;
            if (propertyParser.DefaultValue != null)
            {
                var defaultValue = propertyParser.DefaultValue();
                return new PropertyValue<T>(propertyParser.TargetProperty, defaultValue, ValueSource.DefaultValue);
            }
            return None;
        }
    }

    public class StringParser : ValueParserBase<string>
    {
        public static readonly StringParser Instance = new StringParser();

        /// <inheritdoc />
        public override Option<string> Parse(string source)
        {
            return source;
        }
    }
}
