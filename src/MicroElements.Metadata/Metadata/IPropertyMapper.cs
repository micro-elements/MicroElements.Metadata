// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represent mapper for property.
    /// </summary>
    public interface IPropertyMapper
    {
        /// <summary>
        /// Gets untyped source property.
        /// </summary>
        IProperty SourcePropertyUntyped { get; }

        /// <summary>
        /// Gets untyped target property.
        /// </summary>
        IProperty TargetPropertyUntyped { get; }
    }

    /// <summary>
    /// Generic property mapper.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    public interface IPropertyMapper<TSource, TTarget> : IPropertyMapper
    {
        /// <summary>
        /// Gets source property.
        /// </summary>
        IProperty<TSource> SourceProperty { get; }

        /// <summary>
        /// Gets target property.
        /// </summary>
        IProperty<TTarget> TargetProperty { get; }

        /// <summary>
        /// Gets value parser.
        /// </summary>
        IValueMapper<TSource, TTarget> ValueMapper { get; }
    }
}
