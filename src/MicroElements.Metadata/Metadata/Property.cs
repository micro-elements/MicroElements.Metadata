// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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
            Description = new LocalizableString(name);
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
        public Func<IPropertyContainer, T> Calculate { get; private set; }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// Sets description and returns the same property for builder chaining.
        /// </summary>
        /// <param name="description">Property description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> WithDescription(string description, Language language = Language.Undefined)
        {
            Description = Description.Add(description.Lang(language));
            return this;
        }

        /// <summary>
        /// Sets alias and returns the same property for builder chaining.
        /// </summary>
        /// <param name="alias">Property alias.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> WithAlias(string alias)
        {
            Alias = alias;
            return this;
        }

        /// <summary>
        /// Sets default value and returns the same property for builder chaining.
        /// </summary>
        /// <param name="defaultValue">Func to get property default value.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> WithDefaultValue(Func<T> defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        /// <summary>
        /// Sets default value and returns the same property for builder chaining.
        /// </summary>
        /// <param name="constDefaultValue">Property default value.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> WithDefaultValue(T constDefaultValue)
        {
            DefaultValue = () => constDefaultValue;
            return this;
        }

        /// <summary>
        /// Sets examples for property and returns the same property for builder chaining.
        /// </summary>
        /// <param name="examples">Property example values.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> WithExamples(params T[] examples)
        {
            Examples = examples;
            return this;
        }

        /// <summary>
        /// Sets calculate property value func and returns the same property for builder chaining.
        /// </summary>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>The same property for builder chaining.</returns>
        public Property<T> SetCalculate(Func<IPropertyContainer, T> calculate)
        {
            Calculate = calculate;
            return this;
        }
    }

    /// <summary>
    /// Property extensions.
    /// </summary>
    public static class Property
    {
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
        public static IProperty WithAlias(this IProperty property, string alias)
        {
            static IProperty WithAlias<T>(IProperty property, string alias) => ((Property<T>)property).WithAlias(alias);
            var func = CodeCompiler.CachedCompiledFunc<IProperty, string, IProperty>(property.Type, WithAlias<CodeCompiler.GenericType>);
            return func(property, alias);
        }

        /// <summary>
        /// Sets alias and returns the same property for builder chaining.
        /// NOTE: <paramref name="property"/> should be <see cref="Property{T}"/>.
        /// </summary>
        /// <param name="property">Property to change.</param>
        /// <param name="description">Property description.</param>
        /// <param name="language">Description language.</param>
        /// <returns>The same property for builder chaining.</returns>
        public static IProperty WithDescription(this IProperty property, string description, Language language = Language.Undefined)
        {
            static IProperty WithDescription<T>(IProperty property, string description, Language language) => ((Property<T>)property).WithDescription(description, language);
            var func = CodeCompiler.CachedCompiledFunc<IProperty, string, Language, IProperty>(property.Type, WithDescription<CodeCompiler.GenericType>);
            return func(property, description, language);
        }

        /// <summary>
        /// Sets metadata and returns the same property for builder chaining.
        /// </summary>
        /// <typeparam name="T">Metadata type.</typeparam>
        /// <param name="property">Property to change.</param>
        /// <param name="metadataProperty">Metadata property.</param>
        /// <param name="metadataValue">Metadata value.</param>
        /// <returns>The same property for builder chaining.</returns>
        public static IProperty WithMetadata<T>(this IProperty property, IProperty<T> metadataProperty, T metadataValue)
        {
            if (property.Metadata is IMutablePropertyContainer mutablePropertyContainer)
            {
                mutablePropertyContainer.SetValue(metadataProperty, metadataValue);
            }

            return property;
        }

        /// <summary>
        /// Configures metadata and returns the same property for builder chaining.
        /// </summary>
        /// <param name="property">Property to change.</param>
        /// <param name="configureMetadata">Configure metadata action.</param>
        /// <returns>The same property for builder chaining.</returns>
        public static IProperty ConfigureMetadata(this IProperty property, Action<IMutablePropertyContainer> configureMetadata)
        {
            if (property.Metadata is IMutablePropertyContainer mutablePropertyContainer)
            {
                configureMetadata?.Invoke(mutablePropertyContainer);
            }

            return property;
        }
    }
}
