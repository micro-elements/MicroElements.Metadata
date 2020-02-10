// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property mapper.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    public class PropertyMapper<TSource, TTarget> : IPropertyMapper<TSource, TTarget>
    {
        /// <inheritdoc />
        public IProperty SourcePropertyUntyped => SourceProperty;

        /// <inheritdoc />
        public IProperty TargetPropertyUntyped => TargetProperty;

        /// <inheritdoc />
        public IProperty<TSource> SourceProperty { get; }

        /// <inheritdoc />
        public IProperty<TTarget> TargetProperty { get; }

        /// <inheritdoc />
        public IValueMapper<TSource, TTarget> ValueMapper { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapper{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="sourceProperty">Source property.</param>
        /// <param name="targetProperty">Target property.</param>
        /// <param name="valueMapper">Value mapper.</param>
        public PropertyMapper(IProperty<TSource> sourceProperty, IProperty<TTarget> targetProperty, IValueMapper<TSource, TTarget> valueMapper)
        {
            SourceProperty = sourceProperty.AssertArgumentNotNull(nameof(sourceProperty));
            TargetProperty = targetProperty.AssertArgumentNotNull(nameof(targetProperty));
            ValueMapper = valueMapper.AssertArgumentNotNull(nameof(valueMapper));
        }
    }
}
