// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
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
        /// Initializes a new instance of the <see cref="Property{T}"/> class.
        /// </summary>
        /// <param name="name">Property code.</param>
        public Property(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property{T}"/> class.
        /// Constructor for copying.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="alias">Alias.</param>
        /// <param name="examples">Examples.</param>
        /// <param name="calculator">Calculate func.</param>
        internal Property(
            string name,
            LocalizableString description,
            string alias,
            IReadOnlyList<T> examples,
            IPropertyCalculator<T> calculator)
        {
            Name = name;
            Description = description;
            Alias = alias;
            Examples = examples;
            Calculator = calculator;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; } = typeof(T);

        /// <inheritdoc />
        public LocalizableString Description { get; private set; }

        /// <inheritdoc />
        public string Alias { get; private set; }

        /// <inheritdoc />
        public Func<T> DefaultValue { get; private set; } = () => default;

        /// <inheritdoc />
        public IReadOnlyList<T> Examples { get; private set; }

        /// <inheritdoc />
        public IPropertyCalculator<T> Calculator { get; private set; }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public IEnumerator<IProperty> GetEnumerator()
        {
            yield return this;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Sets description and returns the same property for builder chaining.
        /// </summary>
        /// <param name="description">Property description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetDescription(string description, Language language = Language.Undefined)
        {
            description.AssertArgumentNotNull(nameof(description));
            Description = (Description ?? new LocalizableString()).AddOrReplace(description.Lang(language));
            return this;
        }

        /// <summary>
        /// Sets alias and returns the same property for builder chaining.
        /// </summary>
        /// <param name="alias">Property alias.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetAlias(string alias)
        {
            Alias = alias;
            return this;
        }

        /// <summary>
        /// Sets default value and returns the same property for builder chaining.
        /// </summary>
        /// <param name="defaultValue">Func to get property default value.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetDefaultValue(Func<T> defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        /// <summary>
        /// Sets default value and returns the same property for builder chaining.
        /// </summary>
        /// <param name="constDefaultValue">Property default value.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetDefaultValue(T constDefaultValue)
        {
            DefaultValue = () => constDefaultValue;
            return this;
        }

        /// <summary>
        /// Sets examples for property and returns the same property for builder chaining.
        /// </summary>
        /// <param name="examples">Property example values.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetExamples(params T[] examples)
        {
            Examples = examples;
            return this;
        }

        /// <summary>
        /// Sets property value calculator and returns the same property for builder chaining.
        /// </summary>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetCalculate(Func<IPropertyContainer, T> calculate)
        {
            Calculator = new PropertyCalculator<T>(calculate);
            return this;
        }

        /// <summary>
        /// Sets property value calculator and returns the same property for builder chaining.
        /// </summary>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetCalculate(Func<IPropertyContainer, (T Value, ValueSource ValueSource)> calculate)
        {
            Calculator = new PropertyCalculator<T>(calculate);
            return this;
        }

        /// <summary>
        /// Sets property calculator.
        /// </summary>
        /// <param name="calculator">Property value calculator.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetCalculator(IPropertyCalculator<T> calculator)
        {
            Calculator = calculator;
            return this;
        }
    }

    /// <summary>
    /// Property extensions.
    /// </summary>
    public static class Property
    {
        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> DefaultEqualityComparer = new ByReferenceEqualityComparer();

        /// <summary>
        /// Property comparer by reference equality.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> BeReferenceComparer = new ByReferenceEqualityComparer();

        /// <summary>
        /// Property comparer by <see cref="IProperty.Name"/> and <see cref="IProperty.Type"/>.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByNameAndTypeComparer = new ByNameAndTypeEqualityComparer();

        /// <summary>
        /// Property comparer by <see cref="IProperty.Name"/> or <see cref="IProperty.Alias"/> ignore case.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByNameOrAliasIgnoreCase = ByNameOrAliasEqualityComparer.IgnoreCase;

        /// <summary>
        /// Property comparer by <see cref="IProperty.Name"/> or <see cref="IProperty.Alias"/> ignore case.
        /// </summary>
        public static readonly IEqualityComparer<IProperty> ByNameOrAliasOrdinal = ByNameOrAliasEqualityComparer.Ordinal;

        /// <summary>
        /// Gets comparer ByNameOrAlias depending <paramref name="ignoreCase"/> flag.
        /// </summary>
        /// <param name="ignoreCase">Search ignore case.</param>
        /// <returns>Comparer instance.</returns>
        public static IEqualityComparer<IProperty> ByNameOrAlias(bool ignoreCase) => ignoreCase ? ByNameOrAliasIgnoreCase : ByNameOrAliasOrdinal;

        /// <summary>
        /// Creates new property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
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
        public static IProperty Create(Type type, string name)
        {
            Type typedPropertyType = typeof(Property<>).MakeGenericType(type);
            return (IProperty)Activator.CreateInstance(typedPropertyType, name);
        }

        /// <summary>
        /// Sets alias and returns the same property for builder chaining.
        /// NOTE: <paramref name="property"/> should be <see cref="Property{T}"/>.
        /// </summary>
        /// <param name="property">Property to change.</param>
        /// <param name="alias">Property alias.</param>
        /// <returns>The same property for builder chaining.</returns>
        public static IProperty SetAliasUntyped(this IProperty property, string alias)
        {
            var func = CodeCompiler.CachedCompiledFunc<IProperty, string, IProperty>(property.Type, SetAliasUntyped<CodeCompiler.GenericType>);
            return func(property, alias);
        }

        private static IProperty SetAliasUntyped<T>(IProperty property, string alias) => ((Property<T>)property).SetAlias(alias);

        /// <summary>
        /// Sets alias and returns the same property for builder chaining.
        /// NOTE: <paramref name="property"/> should be <see cref="Property{T}"/>.
        /// </summary>
        /// <param name="property">Property to change.</param>
        /// <param name="description">Property description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>The same property for builder chaining.</returns>
        public static IProperty SetDescriptionUntyped(this IProperty property, string description, Language language = Language.Undefined)
        {
            var func = CodeCompiler.CachedCompiledFunc<IProperty, string, Language, IProperty>(property.Type, SetDescriptionUntyped<CodeCompiler.GenericType>);
            return func(property, description, language);
        }

        private static IProperty SetDescriptionUntyped<T>(IProperty property, string description, Language language) => ((Property<T>)property).SetDescription(description, language);

        /// <summary>
        /// Creates property copy with new name.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="name">New name.</param>
        /// <returns>Property with new name.</returns>
        public static IProperty<T> WithName<T>(this IProperty<T> property, string name)
        {
            property.AssertArgumentNotNull(nameof(property));
            name.AssertArgumentNotNull(nameof(name));

            return new Property<T>(name, property.Description, property.Alias, property.Examples, property.Calculator);
        }

        /// <summary>
        /// Creates property copy with new alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="alias">New alias.</param>
        /// <returns>Property with new alias.</returns>
        public static IProperty<T> WithAlias<T>(this IProperty<T> property, string alias)
        {
            property.AssertArgumentNotNull(nameof(property));
            alias.AssertArgumentNotNull(nameof(alias));

            return new Property<T>(property.Name, property.Description, alias, property.Examples, property.Calculator);
        }

        /// <summary>
        /// Creates property copy with new alias.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="description">New description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>Property with new description.</returns>
        public static Property<T> WithDescription<T>(this IProperty<T> property, string description, Language language = Language.Undefined)
        {
            description.AssertArgumentNotNull(nameof(description));

            var newDescription = new LocalizableString(description.Lang(language));
            return new Property<T>(property.Name, newDescription, property.Alias, property.Examples, property.Calculator);
        }

        /// <summary>
        /// Creates property copy with new default value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="constDefaultValue">New default value.</param>
        /// <returns>Property with new default value.</returns>
        public static IProperty<T> WithDefaultValue<T>(this IProperty<T> property, T constDefaultValue)
        {
            property.AssertArgumentNotNull(nameof(property));

            return new Property<T>(property.Name, property.Description, property.Alias, property.Examples, property.Calculator)
                .SetDefaultValue(constDefaultValue);
        }

        /// <summary>
        /// Creates property copy with new default value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="defaultValue">New default value func.</param>
        /// <returns>Property with new default value func.</returns>
        public static IProperty<T> WithDefaultValue<T>(this IProperty<T> property, Func<T> defaultValue)
        {
            property.AssertArgumentNotNull(nameof(property));

            return new Property<T>(property.Name, property.Description, property.Alias, property.Examples, property.Calculator)
                .SetDefaultValue(defaultValue);
        }

        /// <summary>
        /// Creates new property that copies old property and sets calculate func.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>Property with new calculate func.</returns>
        public static IProperty<T> WithCalculate<T>(this IProperty<T> property, Func<IPropertyContainer, T> calculate)
        {
            property.AssertArgumentNotNull(nameof(property));
            calculate.AssertArgumentNotNull(nameof(calculate));

            return new Property<T>(property.Name, property.Description, property.Alias, property.Examples, new PropertyCalculator<T>(calculate));
        }
    }
}
