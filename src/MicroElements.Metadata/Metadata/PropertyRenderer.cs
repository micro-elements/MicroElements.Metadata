// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyRenderer<T> : IPropertyRenderer<T>
    {
        /// <inheritdoc />
        public IProperty<T> Property { get; private set; }

        /// <inheritdoc />
        public IProperty PropertyUntyped => Property;

        /// <inheritdoc />
        public Type PropertyType => typeof(T);

        /// <inheritdoc />
        public string TargetName { get; private set; }

        /// <summary>
        /// Gets <see cref="SearchOptions"/> for property search.
        /// </summary>
        public SearchOptions? SearchOptions { get; private set; }

        /// <summary>
        /// Gets format function.
        /// </summary>
        public Func<T, IPropertyContainer, string>? FormatValue { get; private set; }

        /// <summary>
        /// Gets value that is renders when property value is null.
        /// </summary>
        public string? NullValue { get; private set; }

        /// <summary>
        /// Gets custom render function that overrides all render.
        /// </summary>
        public Func<IProperty, IPropertyContainer, string>? CustomRender { get; private set; }

        /// <inheritdoc />
        public string Render(IPropertyContainer source)
        {
            if (CustomRender != null)
            {
                return CustomRender(Property, source);
            }

            string? textValue = NullValue;

            IPropertyValue<T>? propertyValue = source.GetPropertyValue(Property, SearchOptions ?? Metadata.SearchOptions.ExistingOnlyWithParent);
            if (propertyValue.HasValue())
            {
                T value = propertyValue.Value;
                textValue = FormatValue?.Invoke(value, source) ?? DoDefaultFormatting(value);
            }

            return textValue;
        }

        /// <inheritdoc />
        public IPropertyRenderer Configure(Action<PropertyRendererOptions> configure)
        {
            if (configure != null)
            {
                var rendererOptions =
                    new PropertyRendererOptions(PropertyUntyped)
                    {
                        TargetName = TargetName,
                        SearchOptions = SearchOptions,
                        NullValue = NullValue,
                        CustomRender = CustomRender,
                    };
                configure(rendererOptions);

                TargetName = rendererOptions.TargetName ?? TargetName;
                SearchOptions = rendererOptions.SearchOptions ?? SearchOptions;
                NullValue = rendererOptions.NullValue ?? NullValue;
                CustomRender = rendererOptions.CustomRender ?? CustomRender;

                rendererOptions.AfterConfigure?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRenderer{T}"/> class.
        /// </summary>
        /// <param name="property">Property to render.</param>
        /// <param name="targetName">Target name.</param>
        public PropertyRenderer(IProperty<T> property, string? targetName = null)
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

        /// <summary>
        /// Sets <see cref="SearchOptions"/> for property search.
        /// </summary>
        /// <param name="searchOptions"><see cref="SearchOptions"/>.</param>
        /// <returns>The same renderer.</returns>
        public PropertyRenderer<T> SetSearchOptions(SearchOptions searchOptions)
        {
            SearchOptions = searchOptions;
            return this;
        }

        private string? DoDefaultFormatting([AllowNull] T value, string? textFormat = null)
        {
            if (value is IFormattable formattable)
            {
                return formattable.ToString(textFormat, CultureInfo.InvariantCulture);
            }

            return value?.ToString() ?? NullValue;
        }
    }

    /// <summary>
    /// <see cref="IPropertyRenderer"/> stuff.
    /// </summary>
    public static class PropertyRenderer
    {
        /// <summary>
        /// Creates <see cref="IPropertyRenderer"/> by property.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="name">Target name.</param>
        /// <returns><see cref="IPropertyRenderer"/> instance.</returns>
        public static IPropertyRenderer Create(IProperty property, string name)
        {
            Type typedPropertyType = typeof(PropertyRenderer<>).MakeGenericType(property.Type);
            return (IPropertyRenderer)Activator.CreateInstance(typedPropertyType, property, name ?? property.Name);
        }

        /// <summary>
        /// Creates renderers by properties.
        /// </summary>
        /// <param name="propertySet">Properties.</param>
        /// <param name="configureRenderer">Configure created renderer.</param>
        /// <returns>Renderers.</returns>
        public static IEnumerable<IPropertyRenderer> ToRenderers(
            this IEnumerable<IProperty> propertySet,
            Action<PropertyRendererOptions>? configureRenderer = null)
        {
            foreach (IProperty property in propertySet)
            {
                IPropertyRenderer propertyRenderer = PropertyRenderer.Create(property, property.Name);
                if (configureRenderer != null)
                    propertyRenderer.Configure(configureRenderer);
                yield return propertyRenderer;
            }
        }

        /// <summary>
        /// Casts typed <see cref="IPropertyRenderer{T}"/> to untyped <see cref="IPropertyRenderer"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="renderer">Source renderer.</param>
        /// <returns>The same renderer casted to untyped form.</returns>
        public static IPropertyRenderer AsUntyped<T>(this IPropertyRenderer<T> renderer) => renderer;

        /// <summary>
        /// Sets <see cref="SearchOptions"/> for <paramref name="renderer"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="searchOptions"><see cref="SearchOptions"/> for property search.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetSearchOptions(this IPropertyRenderer renderer, SearchOptions searchOptions)
        {
            return renderer.Configure(options => options.SearchOptions = searchOptions);
        }

        /// <summary>
        /// Sets render for <see cref="IFormattable"/> objects with text format.
        /// Method creates custom rendering for <see cref="IPropertyRenderer"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="format">Text format.</param>
        /// <param name="formatProvider">Optional <see cref="IFormatProvider"/>.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetFormat(this IPropertyRenderer renderer, string format, IFormatProvider? formatProvider = null)
        {
            return renderer.Configure(options => options.CustomRender = (property, container) => RenderAsFormattable(property, container, options.SearchOptions, format, formatProvider));

            static string RenderAsFormattable(
                IProperty property,
                IPropertyContainer container,
                SearchOptions? searchOptions,
                string format,
                IFormatProvider? formatProvider)
            {
                object? valueUntyped = container.GetPropertyValueUntyped(property, searchOptions)?.ValueUntyped;
                return (valueUntyped as IFormattable)?.ToString(format, formatProvider ?? CultureInfo.InvariantCulture) ?? valueUntyped.DefaultFormatValue();
            }
        }
    }
}
