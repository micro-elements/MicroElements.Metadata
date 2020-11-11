// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property descriptor.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class Property<T> : IProperty<T>
    {
        /// <summary>
        /// Empty property instance.
        /// </summary>
        public static readonly IProperty<T> Empty = new Property<T>(name: "empty");

        [return: MaybeNull]
        private static T GetDefaultValue() => default;

        /// <summary>
        /// Initializes a new instance of the <see cref="Property{T}"/> class.
        /// </summary>
        /// <param name="name">Property code.</param>
        public Property(string name)
        {
            Name = name;
            Description = null;
            Alias = null;
            DefaultValue = GetDefaultValue;
            Examples = Array.Empty<T>();
            Calculator = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property{T}"/> class.
        /// Constructor for copying.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="description">Property description.</param>
        /// <param name="alias">Property alias.</param>
        /// <param name="defaultValue">Default value function.</param>
        /// <param name="examples">Examples.</param>
        /// <param name="calculator">Calculate property value function.</param>
        internal Property(
            string name,
            LocalizableString? description,
            string? alias,
            Func<T> defaultValue,
            IReadOnlyList<T> examples,
            IPropertyCalculator<T>? calculator)
        {
            Name = name;
            Description = description;
            Alias = alias;
            DefaultValue = defaultValue.AssertArgumentNotNull(nameof(defaultValue));
            Examples = examples.AssertArgumentNotNull(nameof(examples));
            Calculator = calculator;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; } = typeof(T);

        /// <inheritdoc />
        public LocalizableString? Description { get; }

        /// <inheritdoc />
        public string? Alias { get; }

        /// <inheritdoc />
        public Func<T> DefaultValue { get; }

        /// <inheritdoc />
        public IReadOnlyList<T> Examples { get; }

        /// <inheritdoc />
        public IPropertyCalculator<T>? Calculator { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// Property extensions.
    /// </summary>
    public static class Property
    {
        /// <summary>
        /// Returns empty property instance.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <returns>Empty property instance.</returns>
        public static IProperty<T> Empty<T>() => Property<T>.Empty;

        /// <summary>
        /// Creates property copy with one or more changes.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="source">Source property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="description">Property description.</param>
        /// <param name="alias">Property alias.</param>
        /// <param name="defaultValue">Default value function.</param>
        /// <param name="examples">Examples.</param>
        /// <param name="calculator">Calculate property value function.</param>
        /// <returns>New instance of <see cref="IProperty{T}"/> with one or more changes.</returns>
        [Pure]
        public static Property<T> With<T>(
            this IProperty<T> source,
            string? name = null,
            LocalizableString? description = null,
            string? alias = null,
            Func<T>? defaultValue = null,
            IReadOnlyList<T>? examples = null,
            IPropertyCalculator<T>? calculator = null)
        {
            source.AssertArgumentNotNull(nameof(source));

            return new Property<T>(
                name: name ?? source.Name,
                alias: alias ?? source.Alias,
                description: description ?? source.Description,
                defaultValue: defaultValue ?? source.DefaultValue,
                examples: examples ?? source.Examples,
                calculator: calculator ?? source.Calculator);
        }

        /// <summary>
        /// Creates new property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
        [Pure]
        public static Property<T> Create<T>(string name)
        {
            return new Property<T>(name);
        }

        /// <summary>
        /// Creates property by <paramref name="type"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Property type.</param>
        /// <param name="name">Property name.</param>
        /// <returns>Created property.</returns>
        [Pure]
        public static IProperty Create(Type type, string name)
        {
            Type typedPropertyType = typeof(Property<>).MakeGenericType(type);
            return (IProperty)Activator.CreateInstance(typedPropertyType, name);
        }

        /// <summary>
        /// Creates property copy with new name.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="name">New name.</param>
        /// <returns>Property with new name.</returns>
        [Pure]
        public static IProperty<T> WithName<T>(this IProperty<T> property, string name)
        {
            property.AssertArgumentNotNull(nameof(property));
            name.AssertArgumentNotNull(nameof(name));

            return property.With(name: name);
        }

        /// <summary>
        /// Creates property copy with new name.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="name">New name.</param>
        /// <returns>Property with new name.</returns>
        [Pure]
        public static IProperty WithNameUntyped(this IProperty property, string name)
        {
            property.AssertArgumentNotNull(nameof(property));
            name.AssertArgumentNotNull(nameof(name));

            static IProperty WithNameCompiled<T>(IProperty property, string name) => ((IProperty<T>)property).WithName(name);

            var withName = CodeCompiler.CachedCompiledFunc<IProperty, string, IProperty>(property.Type, "WithName", WithNameCompiled<CodeCompiler.GenericType>);
            return withName(property, name);
        }

        /// <summary>
        /// Creates property copy with new alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="alias">New alias.</param>
        /// <returns>Property with new alias.</returns>
        [Pure]
        public static IProperty<T> WithAlias<T>(this IProperty<T> property, string alias)
        {
            property.AssertArgumentNotNull(nameof(property));
            alias.AssertArgumentNotNull(nameof(alias));

            return property.With(alias: alias);
        }

        /// <summary>
        /// Creates property copy with new alias.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="alias">New alias.</param>
        /// <returns>Property with new alias.</returns>
        [Pure]
        public static IProperty WithAliasUntyped(this IProperty property, string alias)
        {
            property.AssertArgumentNotNull(nameof(property));
            alias.AssertArgumentNotNull(nameof(alias));

            static IProperty WithAliasCompiled<T>(IProperty property, string name) => ((IProperty<T>)property).WithAlias(name);

            var withAlias = CodeCompiler.CachedCompiledFunc<IProperty, string, IProperty>(property.Type, "WithAlias", WithAliasCompiled<CodeCompiler.GenericType>);
            return withAlias(property, alias);
        }

        /// <summary>
        /// Creates property copy with new description.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="description">New description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>Property with new description.</returns>
        [Pure]
        public static Property<T> WithDescription<T>(this IProperty<T> property, string description, Language language = Language.Undefined)
        {
            property.AssertArgumentNotNull(nameof(property));
            description.AssertArgumentNotNull(nameof(description));

            return property.With(description: new LocalizableString(description.Lang(language)));
        }

        /// <summary>
        /// Creates property copy with updated description.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="description">New description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>Property with new description.</returns>
        [Pure]
        public static Property<T> AddDescription<T>(this IProperty<T> property, string description, Language language = Language.Undefined)
        {
            property.AssertArgumentNotNull(nameof(property));
            description.AssertArgumentNotNull(nameof(description));

            var newDescription = property.Description != null ?
                property.Description.AddOrReplace(description.WithLang(language)) :
                new LocalizableString(description.WithLang(language));

            return property.With(description: newDescription);
        }

        /// <summary>
        /// Creates property copy with new description.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="description">New description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>Property with new description.</returns>
        [Pure]
        public static IProperty WithDescriptionUntyped(this IProperty property, string description, Language language = Language.Undefined)
        {
            property.AssertArgumentNotNull(nameof(property));
            description.AssertArgumentNotNull(nameof(description));

            static IProperty WithDescriptionCompiled<T>(IProperty property, string description, Language language) =>
                ((IProperty<T>)property).WithDescription(description, language);

            var withDescription = CodeCompiler.CachedCompiledFunc<IProperty, string, Language, IProperty>(property.Type, "WithDescription", WithDescriptionCompiled<CodeCompiler.GenericType>);
            return withDescription(property, description, language);
        }

        /// <summary>
        /// Creates property copy with new default value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="constDefaultValue">New default value.</param>
        /// <returns>Property with new default value.</returns>
        [Pure]
        public static IProperty<T> WithDefaultValue<T>(this IProperty<T> property, T constDefaultValue)
        {
            property.AssertArgumentNotNull(nameof(property));

            T GetConstValue() => constDefaultValue;

            return property.With(defaultValue: GetConstValue);
        }

        /// <summary>
        /// Creates property copy with new default value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="defaultValue">New default value func.</param>
        /// <returns>Property with new default value func.</returns>
        [Pure]
        public static IProperty<T> WithDefaultValue<T>(this IProperty<T> property, Func<T> defaultValue)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.With(defaultValue: defaultValue);
        }

        /// <summary>
        /// Creates property copy with new calculator.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>Property with new calculate func.</returns>
        [Pure]
        public static IProperty<T> WithCalculate<T>(this IProperty<T> property, Func<IPropertyContainer, T> calculate)
        {
            property.AssertArgumentNotNull(nameof(property));
            calculate.AssertArgumentNotNull(nameof(calculate));

            return property.With(calculator: new PropertyCalculator<T>(calculate));
        }

        /// <summary>
        /// Creates property copy with new calculator.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>Property with new calculate func.</returns>
        [Pure]
        public static IProperty<T> WithCalculate<T>(this IProperty<T> property, Func<IPropertyContainer, SearchOptions, T> calculate)
        {
            property.AssertArgumentNotNull(nameof(property));
            calculate.AssertArgumentNotNull(nameof(calculate));

            return property.With(calculator: new PropertyCalculator<T>(calculate));
        }

        /// <summary>
        /// Creates property copy with new calculator.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>Property with new calculate func.</returns>
        [Pure]
        public static IProperty<T> WithCalculate<T>(this IProperty<T> property, Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)> calculate)
        {
            property.AssertArgumentNotNull(nameof(property));
            calculate.AssertArgumentNotNull(nameof(calculate));

            return property.With(calculator: new PropertyCalculator<T>(calculate));
        }

        /// <summary>
        /// Creates property copy with new example list.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="examples">Examples for property.</param>
        /// <returns>Property with new example list.</returns>
        [Pure]
        public static IProperty<T> WithExamples<T>(this IProperty<T> property, params T[] examples)
        {
            property.AssertArgumentNotNull(nameof(property));
            examples.AssertArgumentNotNull(nameof(examples));

            return property.With(examples: examples);
        }
    }
}
