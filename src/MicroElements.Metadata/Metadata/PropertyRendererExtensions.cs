// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using MicroElements.Metadata.Formatting;

namespace MicroElements.Metadata
{
    /// <summary>
    /// <see cref="IPropertyRenderer"/> stuff.
    /// </summary>
    public static class PropertyRendererExtensions
    {
        /// <summary>
        /// Creates <see cref="IPropertyRenderer{T}"/> for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="targetName">TargetName for renderer.</param>
        /// <returns><see cref="IPropertyRenderer"/> instance.</returns>
        public static IPropertyRenderer<T> ToRenderer<T>(this IProperty<T> property, string? targetName = null) => new PropertyRenderer<T>(property, targetName: targetName);

        /// <summary>
        /// Creates <see cref="IPropertyRenderer"/> for <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="targetName">TargetName for renderer.</param>
        /// <returns><see cref="IPropertyRenderer"/> instance.</returns>
        public static IPropertyRenderer ToRendererUntyped(this IProperty property, string? targetName = null) => PropertyRenderer.Create(property, targetName);

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
                IPropertyRenderer propertyRenderer = property.ToRendererUntyped();
                if (configureRenderer != null)
                    propertyRenderer = propertyRenderer.ConfigureRenderer(configureRenderer);
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
        /// Executes <paramref name="configure"/> and returns the renderer of the same input type.
        /// <paramref name="configure"/> should return the same renderer or method will throw exception.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">PropertyRenderer type.</typeparam>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="configure">Configure action.</param>
        /// <returns>The same renderer of the same input type.</returns>
        public static TPropertyRenderer ConfigureTyped<TPropertyRenderer>(
            this TPropertyRenderer renderer,
            Func<TPropertyRenderer, IPropertyRenderer> configure)
            where TPropertyRenderer : IPropertyRenderer
        {
            IPropertyRenderer propertyRenderer = configure(renderer);
            if (!ReferenceEquals(propertyRenderer, renderer))
                throw new InvalidOperationException("Configure action should return the same renderer instance");
            return renderer;
        }

        /// <summary>
        /// Sets <see cref="IPropertyRenderer.TargetName"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="targetName">Target name.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetTargetName(this IPropertyRenderer renderer, string targetName)
        {
            return renderer.ConfigureRenderer(options => options.TargetName = targetName);
        }

        /// <summary>
        /// Sets <see cref="SearchOptions"/> for <paramref name="renderer"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="searchOptions"><see cref="SearchOptions"/> for property search.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetSearchOptions(this IPropertyRenderer renderer, SearchOptions searchOptions)
        {
            return renderer.ConfigureRenderer(options => options.SearchOptions = searchOptions);
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
            return renderer.ConfigureRenderer(options =>
            {
                options.CustomRender = (property, container) => RenderAsFormattable(property, container, options, format, formatProvider);
            });

            static string? RenderAsFormattable(
                IProperty property,
                IPropertyContainer container,
                PropertyRendererOptions options,
                string format,
                IFormatProvider? formatProvider)
            {
                SearchOptions searchOptions = options.SearchOptions ?? container.SearchOptions.UseDefaultValue(false).ReturnNull();
                object? valueUntyped = container.GetValueUntyped(property, searchOptions);

                if (valueUntyped == null)
                {
                    return options.NullValue;
                }

                if (valueUntyped is IFormattable formattable)
                {
                    return formattable.ToString(format, formatProvider ?? CultureInfo.InvariantCulture);
                }

                return valueUntyped.FormatValue(nullPlaceholder: options.NullValue);
            }
        }

        /// <summary>
        /// Sets rendering with provided <paramref name="formatValue"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="formatValue">Func that formats value.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer<T> SetFormat<T>(this IPropertyRenderer<T> renderer, Func<T, IPropertyContainer, string?> formatValue)
        {
            return (IPropertyRenderer<T>)renderer.ConfigureRenderer(options =>
            {
                options.CustomRender = (property, container) => RenderWithFunc(container, options, formatValue);
            });

            static string? RenderWithFunc(
                IPropertyContainer container,
                PropertyRendererOptions options,
                Func<T, IPropertyContainer, string?> formatFunc)
            {
                IPropertyValue<T>? propertyValue = container.GetPropertyValue((IProperty<T>)options.PropertyUntyped, options.SearchOptions);
                if (propertyValue.HasValue())
                {
                    T value = propertyValue.Value;
                    return formatFunc(value, container);
                }

                return options.NullValue;
            }
        }

        /// <summary>
        /// Sets <see cref="PropertyRendererOptions.NullValue"/> that renders when property value is null.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <param name="nullValue">Null value.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetNullValue(this IPropertyRenderer renderer, string? nullValue)
        {
            return renderer.ConfigureRenderer(options => options.NullValue = nullValue);
        }

        /// <summary>
        /// Sets <see cref="IPropertyRenderer.TargetName"/> from <see cref="IHasAlias.Alias"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetNameFromAlias(this IPropertyRenderer renderer)
        {
            return renderer.ConfigureRenderer(options =>
            {
                if (options.PropertyUntyped.GetAlias() is { } alias)
                    options.TargetName = alias;
            });
        }
    }
}
