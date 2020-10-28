// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent property renderer.
    /// </summary>
    public interface IPropertyRenderer : IMetadataProvider
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        IProperty PropertyUntyped { get; }

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

        /// <summary>
        /// Configures renderer.
        /// Can be called many times.
        /// </summary>
        /// <param name="configure">Action to configure renderer.</param>
        /// <returns>Returns the same renderer instance for chaining.</returns>
        IPropertyRenderer Configure(Action<PropertyRendererOptions> configure);
    }

    /// <summary>
    /// Generic property renderer.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyRenderer<T> : IPropertyRenderer
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        IProperty<T> Property { get; }
    }

    /// <summary>
    /// Options to configure <see cref="IPropertyRenderer"/>.
    /// </summary>
    public class PropertyRendererOptions
    {
        /// <summary>
        /// Gets property to render.
        /// </summary>
        public IProperty PropertyUntyped { get; }

        /// <summary>
        /// Gets or sets target name.
        /// </summary>
        public string? TargetName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="SearchOptions"/> for property search.
        /// </summary>
        public SearchOptions? SearchOptions { get; set; }

        /// <summary>
        /// Gets or sets value that is renders when property value is null.
        /// </summary>
        public string? NullValue { get; set; }

        /// <summary>
        /// Gets or sets CustomRender function that overrides entire Render for renderer.
        /// </summary>
        public Func<IProperty, IPropertyContainer, string>? CustomRender { get; set; }

        /// <summary>
        /// Callback that invokes after configuring.
        /// </summary>
        public Action<IPropertyRenderer>? AfterConfigure { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRendererOptions"/> class.
        /// </summary>
        /// <param name="propertyUntyped">Property to render.</param>
        public PropertyRendererOptions(IProperty propertyUntyped)
        {
            PropertyUntyped = propertyUntyped;
        }
    }
}
