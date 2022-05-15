// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static partial class SchemaBuilder
    {
        /// <summary>
        /// Creates a new copy of the source with new <see cref="IPropertyCalculator{T}"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="calculator">Property calculator.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithPropertyCalculator<TSchema, T>(this TSchema schema, IPropertyCalculator<T> calculator)
            where TSchema : ISchemaBuilder<IPropertyCalculator<T>>, ISchema<T>
        {
            return WithComponent(schema, calculator);
        }

        /// <summary>
        /// Creates property copy with new calculator.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="calculate">Calculate property value func.</param>
        /// <returns>Property with new calculate func.</returns>
        [Pure]
        public static TSchema WithCalculate<TSchema, T>(this TSchema property, Func<IPropertyContainer, T> calculate)
            where TSchema : ISchemaBuilder<IPropertyCalculator<T>>, ISchema<T>
        {
            return property.WithPropertyCalculator(new PropertyCalculator<T>(calculate));
        }

        /// <inheritdoc cref="WithCalculate{TSchema,T}(TSchema,Func{IPropertyContainer,T})"/>
        [Pure]
        public static TSchema WithCalculate<TSchema, T>(this TSchema property, Func<IPropertyContainer, SearchOptions, T> calculate)
            where TSchema : ISchemaBuilder<IPropertyCalculator<T>>, ISchema<T>
        {
            return property.WithPropertyCalculator(new PropertyCalculator<T>(calculate));
        }

        /// <inheritdoc cref="WithCalculate{TSchema,T}(TSchema,Func{IPropertyContainer,T})"/>
        [Pure]
        public static TSchema WithCalculate<TSchema, T>(this TSchema property, Func<IPropertyContainer, SearchOptions, (T Value, ValueSource ValueSource)> calculate)
            where TSchema : ISchemaBuilder<IPropertyCalculator<T>>, ISchema<T>
        {
            return property.WithPropertyCalculator(new PropertyCalculator<T>(calculate));
        }
    }
}
