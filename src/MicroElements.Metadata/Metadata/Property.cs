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
        /// <param name="code">Property code.</param>
        public Property(string code)
        {
            Name = code;
            Description = new LocalizableString(code);
        }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Metadata { get; } = new PropertyList();

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; } = typeof(T);

        /// <inheritdoc />
        public LocalizableString Description { get; private set; }

        /// <inheritdoc />
        public string Alias { get; private set; }

        /// <inheritdoc />
        public Func<T> DefaultValue { get; private set; } = () => default(T);

        /// <inheritdoc />
        public IReadOnlyList<T> Examples { get; private set; }

        /// <inheritdoc />
        public Func<IPropertyContainer, T> Calculate { get; private set; }

        /// <inheritdoc />
        public override string ToString() => Name;

        public Property<T> WithDescription(string description, Language language)
        {
            Description = Description.Add(description.Lang(language));
            return this;
        }

        public Property<T> WithAlias(string alias)
        {
            Alias = alias;
            return this;
        }

        public Property<T> WithDefaultValue(Func<T> defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        public Property<T> WithExamples(params T[] examples)
        {
            Examples = examples;
            return this;
        }

        public Property<T> SetCalculate(Func<IPropertyContainer, T> evaluate)
        {
            Calculate = evaluate;
            return this;
        }

        
    }
}
