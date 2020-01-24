// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Strong typed property and value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class PropertyValue<T> : IPropertyValue<T>
    {
        /// <inheritdoc />
        public IProperty<T> Property { get; }

        /// <inheritdoc />
        public T Value { get; }

        /// <inheritdoc />
        public IProperty PropertyUntyped => Property;

        /// <inheritdoc />
        public object ValueUntyped => Value;

        /// <inheritdoc />
        public ValueSource Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValue{T}"/> class.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="value">Value for property.</param>
        /// <param name="source">The source for value.</param>
        public PropertyValue(IProperty<T> property, T value, ValueSource source = ValueSource.Defined)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
            Value = value;
            Source = source;
        }

        /// <summary>
        /// Implicitly converts to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="propertyValue">PropertyValue.</param>
        public static implicit operator T(PropertyValue<T> propertyValue) => propertyValue.Value;

        /// <inheritdoc />
        public override string ToString() => $"{Property.Code}: {Value.FormatValue()}";
    }

    /// <summary>
    /// Value source.
    /// </summary>
    public enum ValueSource
    {
        /// <summary>
        /// Value is not set.
        /// </summary>
        NotDefined,

        /// <summary>
        /// Value is defined.
        /// </summary>
        Defined,

        /// <summary>
        /// Value was calculated.
        /// </summary>
        Calculated,

        /// <summary>
        /// Value is default value for property.
        /// </summary>
        DefaultValue,
    }
}
