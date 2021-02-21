// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Strong typed property and value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public sealed class PropertyValue<T> : IPropertyValue<T>
    {
        /// <inheritdoc />
        public IProperty<T> Property { get; }

        /// <inheritdoc />
        [MaybeNull]
        [AllowNull]
        public T Value { get; }

        /// <inheritdoc />
        public IProperty PropertyUntyped => Property;

        /// <inheritdoc />
        public object? ValueUntyped => Value;

        /// <inheritdoc />
        public ValueSource Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValue{T}"/> class.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="value">Value for property.</param>
        /// <param name="source">The source for value.</param>
        public PropertyValue(IProperty<T> property, [AllowNull] T value, ValueSource? source = null)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Value = value;
            Source = source ?? ValueSource.Defined;
        }

        /// <summary>
        /// Implicitly converts to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="propertyValue">PropertyValue.</param>
        [return: MaybeNull]
        public static implicit operator T(PropertyValue<T> propertyValue) => propertyValue.Value;

        /// <inheritdoc />
        public override string ToString() => $"{Property.Name}: {Value.FormatValue()}";
    }

    /// <summary>
    /// Represents value source.
    /// </summary>
    public sealed class ValueSource
    {
        /// <summary>
        /// Value is not set.
        /// </summary>
        public static readonly ValueSource NotDefined = new ValueSource("NotDefined");

        /// <summary>
        /// Value is defined.
        /// </summary>
        public static readonly ValueSource Defined = new ValueSource("Defined");

        /// <summary>
        /// Value was calculated.
        /// </summary>
        public static readonly ValueSource Calculated = new ValueSource("Calculated");

        /// <summary>
        /// Value is default value for property.
        /// </summary>
        public static readonly ValueSource DefaultValue = new ValueSource("DefaultValue");

        /// <summary>
        /// Gets the source name.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSource"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        public ValueSource(string sourceName)
        {
            SourceName = sourceName.AssertArgumentNotNull(nameof(sourceName));
        }

        private bool Equals(ValueSource other) => SourceName == other.SourceName;

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is ValueSource other && Equals(other));

        /// <inheritdoc />
        public override int GetHashCode() => SourceName.GetHashCode();

        public static bool operator ==(ValueSource? left, ValueSource? right) => Equals(left, right);

        public static bool operator !=(ValueSource? left, ValueSource? right) => !Equals(left, right);

        /// <inheritdoc />
        public override string ToString() => SourceName;
    }

    /// <summary>
    /// PropertyValue extensions.
    /// </summary>
    public static class PropertyValue
    {
        /// <summary>
        /// Creates new instance if <see cref="IPropertyValue"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value for property.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>Created property value.</returns>
        public static IPropertyValue Create(IProperty property, object? value, ValueSource? valueSource = null)
        {
            return PropertyValueFactory.Default.CreateUntyped(property, value, valueSource);
        }

        public static IPropertyValue Default(IProperty property)
        {
            return PropertyValueFactory.Default.CreateUntyped(property, property.Type.GetDefaultValue(), ValueSource.DefaultValue);
        }

        public static IPropertyValue NotDefined(IProperty property)
        {
            return PropertyValueFactory.Default.CreateUntyped(property, property.Type.GetDefaultValue(), ValueSource.NotDefined);
        }
    }
}
