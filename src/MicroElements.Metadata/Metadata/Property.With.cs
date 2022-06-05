// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.CodeCompiler;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property extensions.
    /// </summary>
    public static partial class Property
    {
        /// <summary>
        /// Creates the property copy with one or more changes.
        /// This method can not set <see langword="null"/>.
        /// Use <see cref="WithRewrite{T}"/> method if you want so set any property.
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
            string? description = null,
            string? alias = null,
            IDefaultValue<T>? defaultValue = null,
            IExamples<T>? examples = null,
            IPropertyCalculator<T>? calculator = null)
        {
            source.AssertArgumentNotNull(nameof(source));

            var property = new Property<T>(
                name: name ?? source.Name,
                alias: alias ?? source.Alias,
                description: description ?? source.Description,
                defaultValue: defaultValue ?? source.DefaultValue,
                examples: examples ?? source.Examples,
                calculator: calculator ?? source.Calculator);

            source.CopyMetadataTo(property);

            return property;
        }

        /// <summary>
        /// Creates a copy of the property.
        /// Allows to set (replace) any property value including <langword>null</langword>.
        /// Method <see cref="With{T}"/> can only set not null values.
        /// </summary>
        /// <typeparam name="T">Property data type.</typeparam>
        /// <param name="source">The source property.</param>
        /// <param name="configure">Configure action.</param>
        /// <returns>New copy of the source property.</returns>
        [Pure]
        public static Property<T> WithRewriteFast<T>(
            this IProperty<T> source,
            ConfigureProperty<T> configure)
        {
            source.AssertArgumentNotNull(nameof(source));

            var sourceData = new PropRefData<T>
            {
                Name = source.Name,
                Description = source.Description,
                Alias = source.Alias,
                DefaultValue = source.DefaultValue,
                Examples = source.Examples,
                Calculator = source.Calculator,
            };

            configure(ref sourceData);

            var property = new Property<T>(
                name: sourceData.Name,
                description: sourceData.Description,
                alias: sourceData.Alias,
                defaultValue: sourceData.DefaultValue,
                examples: sourceData.Examples,
                calculator: sourceData.Calculator);

            source.CopyMetadataTo(property);

            return property;
        }

        /// <summary>
        /// Creates a copy of the property.
        /// Allows to set (replace) any property value including <langword>null</langword>.
        /// Method <see cref="With{T}"/> can only set not null values.
        /// </summary>
        /// <typeparam name="T">Property data type.</typeparam>
        /// <param name="source">The source property.</param>
        /// <param name="configure">Configure action.</param>
        /// <returns>New copy of the source property.</returns>
        [Pure]
        public static Property<T> WithRewrite<T>(
            this IProperty<T> source,
            Action<PropertyData<T>> configure)
        {
            source.AssertArgumentNotNull(nameof(source));
            configure.AssertArgumentNotNull(nameof(configure));

            var sourceData = new PropertyData<T>
            {
                Name = source.Name,
                Description = source.Description,
                Alias = source.Alias,
                DefaultValue = source.DefaultValue,
                Examples = source.Examples,
                Calculator = source.Calculator,
            };

            configure(sourceData);

            var property = new Property<T>(
                name: sourceData.Name,
                description: sourceData.Description,
                defaultValue: sourceData.DefaultValue,
                examples: sourceData.Examples,
                calculator: sourceData.Calculator,
                alias: sourceData.Alias);

            source.CopyMetadataTo(property);

            return property;
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

            return property.With(examples: new ExampleList<T>(examples));
        }
    }

    /// <summary>
    /// Represents a mutable copy of <see cref="Property{T}"/> internal state.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyData<T>
    {
        /// <summary>Sets the property name.</summary>
        public string Name;

        /// <summary>Sets the property description.</summary>
        public string? Description;

        /// <summary>Sets the property alias.</summary>
        public string? Alias;

        /// <summary>Sets the property default value.</summary>
        public IDefaultValue<T>? DefaultValue;

        /// <summary>Sets the property calculator.</summary>
        public IPropertyCalculator<T>? Calculator;

        /// <summary>Sets the property examples.</summary>
        public IExamples<T>? Examples;
    }

    /// <summary>
    /// Represents a mutable copy of <see cref="Property{T}"/> internal state as a value ref struct.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public ref struct PropRefData<T>
    {
        /// <summary>Sets the property name.</summary>
        public string Name;

        /// <summary>Sets the property description.</summary>
        public string? Description;

        /// <summary>Sets the property alias.</summary>
        public string? Alias;

        /// <summary>Sets the property default value.</summary>
        public IDefaultValue<T>? DefaultValue;

        /// <summary>Sets the property calculator.</summary>
        public IPropertyCalculator<T>? Calculator;

        /// <summary>Sets the property examples.</summary>
        public IExamples<T>? Examples;
    }

    /// <summary> Configure property delegate. </summary>
    public delegate void ConfigureProperty<T>(ref PropRefData<T> data);
}
