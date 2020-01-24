// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent property renderer.
    /// </summary>
    public interface IPropertyRenderer : IMetadataProvider
    {
        /// <summary>
        /// Gets property type.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets target name.
        /// </summary>
        string TargetName { get; }

        /// <summary>
        /// Method to render property as text value.
        /// </summary>
        /// <param name="source">Source object to render.</param>
        /// <returns>Rendered text value.</returns>
        string Render(IPropertyContainer source);
    }

    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyRenderer<out T> : IPropertyRenderer
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        IProperty<T> Property { get; }
    }

    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyRenderer<T> : IPropertyRenderer<T>
    {
        private readonly Dictionary<Type, object> _metadata = new Dictionary<Type, object>();

        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public string TargetName { get; set; }

        /// <inheritdoc />
        public IProperty<T> Property { get; set; }

        /// <inheritdoc />
        IReadOnlyDictionary<Type, object> IMetadataProvider.Metadata => _metadata;

        /// <summary>
        /// Gets format function.
        /// </summary>
        public Func<T, IPropertyContainer, string> FormatValue { get; private set; }

        /// <inheritdoc />
        public string Render(IPropertyContainer source)
        {
            var value = source.GetValue(Property);
            var textValue = FormatValue != null ? FormatValue(value, source) : $"{value}";
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
            TargetName = targetName ?? property.Code;
        }

        /// <summary>
        /// Sets metadata for item.
        /// </summary>
        /// <typeparam name="TData">Metadata type.</typeparam>
        /// <param name="data">Metadata.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetMeta<TData>(TData data)
        {
            _metadata[typeof(TData)] = data;
            return this;
        }

        /// <summary>
        /// Sets text format for <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetFormat(string format)
        {
            FormatValue = (value, pc) => Format(value, format, pc);
            return this;
        }

        public PropertyRenderer<T> SetFormat(Func<T, IPropertyContainer, string> formatFunc)
        {
            FormatValue = formatFunc;
            return this;
        }

        public PropertyRenderer<T> SetName(string name)
        {
            TargetName = name;
            return this;
        }

        private string Format(T value, string textFormat, IPropertyContainer propertyContainer)
        {
            if (textFormat != null && value is IFormattable formattable)
            {
                return formattable.ToString(textFormat, CultureInfo.InvariantCulture);
            }

            return value?.ToString() ?? "null";
        }
    }
}
