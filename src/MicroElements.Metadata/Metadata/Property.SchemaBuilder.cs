// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property schema builder implementation.
    /// </summary>
    public partial class Property<T> :
        ISchemaBuilder<Property<T>, ISchemaDescription>,
        ISchemaBuilder<Property<T>, IDefaultValue<T>>,
        ISchemaBuilder<Property<T>, IDefaultValue>,
        ISchemaBuilder<Property<T>, IPropertyCalculator<T>>,
        ISchemaBuilder<Property<T>, IExamples<T>>
    {
        /// <inheritdoc />
        public Property<T> With(ISchemaDescription schemaDescription)
        {
            return this.WithRewriteFast((ref PropertyData<T> data) => data.Description = schemaDescription.Description);
        }

        /// <inheritdoc />
        public Property<T> With(IDefaultValue<T> defaultValue)
        {
            return this.WithRewriteFast((ref PropertyData<T> data) => data.DefaultValue = defaultValue);
        }

        /// <inheritdoc />
        Property<T> ICompositeBuilder<Property<T>, IDefaultValue>.With(IDefaultValue defaultValueUntyped)
        {
            if (defaultValueUntyped is IDefaultValue<T> defaultValueTyped)
                return With(defaultValueTyped);

            object? valueUntyped = defaultValueUntyped.Value.ThrowIfValueCanNotBeAssignedToType(Type);
            IDefaultValue<T> defaultValue = new DefaultValue<T>((T?)valueUntyped);

            return With(defaultValue);
        }

        /// <inheritdoc />
        public Property<T> With(IPropertyCalculator<T> calculator)
        {
            return this.WithRewriteFast((ref PropertyData<T> data) => data.Calculator = calculator);
        }

        /// <inheritdoc />
        public Property<T> With(IExamples<T> examples)
        {
            return this.WithRewriteFast((ref PropertyData<T> data) => data.Examples = examples);
        }
    }
}
