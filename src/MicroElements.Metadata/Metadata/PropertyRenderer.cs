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
        public IProperty<T> Property { get; }

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
        /// Gets custom render function that overrides all render.
        /// </summary>
        public Func<IProperty, IPropertyContainer, string?>? CustomRender { get; private set; }

        /// <summary>
        /// Gets value that is renders when property value is null.
        /// </summary>
        public string? NullValue { get; private set; }

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

        /// <inheritdoc />
        public string? Render(IPropertyContainer source)
        {
            if (CustomRender != null)
            {
                return CustomRender(Property, source) ?? NullValue;
            }

            IPropertyValue<T>? propertyValue = source.GetPropertyValue(Property, SearchOptions);
            if (propertyValue.HasValue())
            {
                return propertyValue.Value?.FormatValue() ?? NullValue;
            }

            return NullValue;
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
        public static IPropertyRenderer Create(IProperty property, string? name)
        {
            Type typedPropertyType = typeof(PropertyRenderer<>).MakeGenericType(property.Type);
            return (IPropertyRenderer)Activator.CreateInstance(typedPropertyType, property, name ?? property.Name);
        }
    }

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
                    propertyRenderer = propertyRenderer.Configure(configureRenderer);
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
            return renderer.Configure(options => options.TargetName = targetName);
        }

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
            return renderer.Configure(options =>
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
                object? valueUntyped = container.GetPropertyValueUntyped(property, options.SearchOptions)?.ValueUntyped;

                if (valueUntyped == null)
                {
                    return options.NullValue;
                }

                if (valueUntyped is IFormattable formattable)
                {
                    return formattable.ToString(format, formatProvider ?? CultureInfo.InvariantCulture);
                }

                return valueUntyped.DefaultFormatValue();
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
            return (IPropertyRenderer<T>)renderer.Configure(options =>
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
            return renderer.Configure(options => options.NullValue = nullValue);
        }

        /// <summary>
        /// Sets <see cref="IPropertyRenderer.TargetName"/> from <see cref="IProperty.Alias"/>.
        /// </summary>
        /// <param name="renderer">Source renderer.</param>
        /// <returns>The same renderer for chaining.</returns>
        public static IPropertyRenderer SetNameFromAlias(this IPropertyRenderer renderer)
        {
            return renderer.Configure(options => options.TargetName = options.PropertyUntyped.Alias);
        }
    }
}
