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
        IConfigurableBuilder<Property<T>, PropertyData<T>>,

        IPropertyBuilder<T>,

        ISchemaBuilder<Property<T>, INameAlias>,
        ISchemaBuilder<Property<T>, ISchemaDescription>,
        ISchemaBuilder<Property<T>, IDefaultValue<T>>,
        ISchemaBuilder<Property<T>, IDefaultValue>,
        ISchemaBuilder<Property<T>, IPropertyCalculator<T>>,
        ISchemaBuilder<Property<T>, IExamples<T>>
    {
        /// <inheritdoc />
        public PropertyData<T> GetState()
        {
            Property<T> source = this;
            return new PropertyData<T>
            {
                Name = source.Name,
                Description = source.Description,
                Alias = source.Alias,
                DefaultValue = source.DefaultValue,
                Examples = source.Examples,
                Calculator = source.Calculator,
            };
        }

        /// <inheritdoc />
        public Property<T> With(PropertyData<T> propertyData)
        {
            var property = new Property<T>(
                    name: propertyData.Name,
                    description: propertyData.Description,
                    defaultValue: propertyData.DefaultValue,
                    examples: propertyData.Examples,
                    calculator: propertyData.Calculator,
                    alias: propertyData.Alias)
                .SetMetadataFrom(this);

            return property;
        }

        /// <inheritdoc />
        public Property<T> With(INameAlias nameAlias)
        {
            return this.WithRewriteFast((ref PropRefData<T> data) => data.Alias = nameAlias.Alias);
        }

        /// <inheritdoc />
        public Property<T> With(ISchemaDescription schemaDescription)
        {
            return this.WithRewriteFast((ref PropRefData<T> data) => data.Description = schemaDescription.Description);
        }

        /// <inheritdoc />
        public Property<T> With(IDefaultValue<T> defaultValue)
        {
            return this.WithRewriteFast((ref PropRefData<T> data) => data.DefaultValue = defaultValue);
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
            return this.WithRewriteFast((ref PropRefData<T> data) => data.Calculator = calculator);
        }

        /// <inheritdoc />
        public Property<T> With(IExamples<T> examples)
        {
            return this.WithRewriteFast((ref PropRefData<T> data) => data.Examples = examples);
        }
    }
}
