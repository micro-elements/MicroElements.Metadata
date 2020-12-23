// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
        public IPropertyRenderer ConfigureRenderer(Action<PropertyRendererOptions> configure)
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
}
