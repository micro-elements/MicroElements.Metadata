// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyRenderer<T> : IPropertyRenderer<T>
    {
        /// <inheritdoc />
        public IReadOnlyList<IPropertyValue> Metadata { get; } = new MutablePropertyContainer();

        /// <inheritdoc />
        public IProperty<T> Property { get; set; }

        /// <inheritdoc />
        public IProperty PropertyUntyped => Property;

        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public string TargetName { get; set; }

        /// <summary>
        /// Gets format function.
        /// </summary>
        public Func<T, IPropertyContainer, string> FormatValue { get; private set; }

        /// <summary>
        /// NullValue is renders when property value is null.
        /// </summary>
        public string NullValue { get; private set; }

        /// <inheritdoc />
        public string Render(IPropertyContainer source)
        {
            var value = source.GetValue(Property);
            var textValue = FormatValue?.Invoke(value, source) ?? DoDefaultFormatting(value);
            return textValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRenderer{T}"/> class.
        /// </summary>
        /// <param name="property">Property to render.</param>
        /// <param name="targetName">Target name.</param>
        public PropertyRenderer(IProperty<T> property, string targetName = null)
        {
            Property = property;
            TargetName = targetName ?? property.Name;
        }

        /// <summary>
        /// Sets text format for <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetFormat(string format)
        {
            FormatValue = (value, pc) => DoDefaultFormatting(value, format);
            return this;
        }

        /// <summary>
        /// Sets formatting func for property renderer.
        /// </summary>
        /// <param name="formatFunc">Func that renders property value.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetFormat(Func<T, IPropertyContainer, string> formatFunc)
        {
            FormatValue = formatFunc;
            return this;
        }

        /// <summary>
        /// Sets target name.
        /// </summary>
        /// <param name="targetName">Target name.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetTargetName(string targetName)
        {
            TargetName = targetName;
            return this;
        }

        /// <summary>
        /// Sets NullValue that renders when property value is null.
        /// </summary>
        /// <param name="nullValue">NullValue.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetNullValue(string nullValue)
        {
            NullValue = nullValue;
            return this;
        }

        private string DoDefaultFormatting(T value, string textFormat = null)
        {
            if (textFormat != null && value is IFormattable formattable)
            {
                return formattable.ToString(textFormat, CultureInfo.InvariantCulture);
            }

            return value?.ToString() ?? NullValue;
        }
    }
}
