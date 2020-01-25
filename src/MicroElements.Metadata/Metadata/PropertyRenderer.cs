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
        public IReadOnlyList<IPropertyValue> Metadata { get; } = new PropertyList();

        /// <inheritdoc />
        public IProperty<T> Property { get; set; }

        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public string TargetName { get; set; }

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
            TargetName = targetName ?? property.Name;
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
