// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property description.
    /// </summary>
    public interface IProperty :
        ISchema,

        ISchemaDescription,
        IHas<ISchemaDescription>,
        ISchemaBuilder<ISchemaDescription>,

        INameAlias,
        IHas<INameAlias>,

        ISchemaBuilder<IDefaultValue>
    {
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// TODO: make IProperty covariant?
    public interface IProperty<T> :
        IProperty,
        ISchema<T>,

        IHas<IExamples<T>>,
        ISchemaBuilder<IExamples<T>>,

        IHas<IDefaultValue<T>>,
        ISchemaBuilder<IDefaultValue<T>>,

        IHas<IPropertyCalculator<T>>,
        ISchemaBuilder<IPropertyCalculator<T>>
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        IDefaultValue<T>? DefaultValue { get; }

        /// <summary>
        /// Gets property value calculator.
        /// </summary>
        IPropertyCalculator<T>? Calculator { get; }

        /// <summary>
        /// Gets property example values.
        /// </summary>
        IExamples<T>? Examples { get; }

        /// <inheritdoc />
        ISchemaDescription? IHas<ISchemaDescription>.Component => Description != null ? SchemaDescription.FromStringCached(Description) : null;

        /// <inheritdoc />
        INameAlias? IHas<INameAlias>.Component => Alias != null ? NameAlias.FromStringCached(Alias) : null;

        /// <inheritdoc />
        IDefaultValue<T>? IHas<IDefaultValue<T>>.Component => DefaultValue;

        /// <inheritdoc />
        IPropertyCalculator<T>? IHas<IPropertyCalculator<T>>.Component => Calculator;

        /// <inheritdoc />
        IExamples<T>? IHas<IExamples<T>>.Component => Examples;
    }
}
