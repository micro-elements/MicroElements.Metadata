// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property descriptor.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    [DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
    public sealed partial class Property<T> : IProperty<T>
    {
        /// <summary>
        /// Empty property instance.
        /// </summary>
        public static readonly IProperty<T> Empty = new Property<T>(name: "empty");

        /// <summary>
        /// Initializes a new instance of the <see cref="Property{T}"/> class.
        /// </summary>
        /// <param name="name">Property code.</param>
        public Property(string name)
        {
            Name = name;

            // Other properties can be configured by method With
            Description = null;
            Alias = null;
            DefaultValue = null;
            Examples = null;
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
            string? description,
            string? alias,
            IDefaultValue<T>? defaultValue,
            IExamples<T>? examples,
            IPropertyCalculator<T>? calculator)
        {
            Name = name;

            Description = description;
            Alias = alias;
            DefaultValue = defaultValue;
            Examples = examples;
            Calculator = calculator;
        }

        /// <inheritdoc />
        public Type Type { get; } = typeof(T);

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <inheritdoc />
        public string? Alias { get; }

        /// <inheritdoc />
        public IDefaultValue<T>? DefaultValue { get; }

        /// <inheritdoc />
        public IExamples<T>? Examples { get; }

        /// <inheritdoc />
        public IPropertyCalculator<T>? Calculator { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// Property extensions.
    /// </summary>
    public static partial class Property
    {
        /// <summary>
        /// Returns empty property instance.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <returns>Empty property instance.</returns>
        public static IProperty<T> Empty<T>() => Property<T>.Empty;

        /// <summary>
        /// Creates new property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="name">Property name.</param>
        /// <returns>Property.</returns>
        [Pure]
        public static IProperty<T> Create<T>(string name)
        {
            return PropertyFactory.Default.Create<T>(name);
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
            return PropertyFactory.Default.Create(type, name);
        }
    }
}
