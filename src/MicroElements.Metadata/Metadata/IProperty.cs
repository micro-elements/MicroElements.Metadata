// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        ISchemaDescription? IHas<ISchemaDescription>.Component => Description != null ? SchemaDescription.FromStringCached(Description) : this.GetMetadata<ISchemaDescription>();

        /// <inheritdoc />
        INameAlias? IHas<INameAlias>.Component => Alias != null ? NameAlias.FromStringCached(Alias) : this.GetMetadata<INameAlias>();

        /// <inheritdoc />
        IDefaultValue<T>? IHas<IDefaultValue<T>>.Component => DefaultValue ?? this.GetMetadata<IDefaultValue<T>>();

        /// <inheritdoc />
        IPropertyCalculator<T>? IHas<IPropertyCalculator<T>>.Component => Calculator ?? this.GetMetadata<IPropertyCalculator<T>>();

        /// <inheritdoc />
        IExamples<T>? IHas<IExamples<T>>.Component => Examples ?? this.GetMetadata<IExamples<T>>();
    }
}
