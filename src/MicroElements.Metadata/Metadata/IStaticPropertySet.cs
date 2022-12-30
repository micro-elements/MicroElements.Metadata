// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Static property set gets properties from static fields and properties so you should not to implement <see cref="IPropertySet.GetProperties"/>.
    /// </summary>
    public interface IStaticPropertySet : IPropertySet
    {
        /// <inheritdoc />
        IEnumerable<IProperty> IPropertySet.GetProperties() => GetType().GetStaticProperties();
    }

    /// <summary>
    /// Static property set gets properties from static fields and properties so you should not to implement <see cref="IPropertySet.GetProperties"/>.
    /// </summary>
    public class StaticPropertySet : IStaticPropertySet
    {
        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties() => GetType().GetStaticProperties();
    }

    public interface IOneOf : ISchemaComponent
    {
        public IEnumerable<ISchema> OneOf();
    }

    public class OneOfComponent : IOneOf
    {
        private IReadOnlyCollection<ISchema> Schemas { get; }

        public OneOfComponent(IReadOnlyCollection<ISchema> schemas)
        {
            Schemas = schemas;
        }

        /// <inheritdoc />
        public IEnumerable<ISchema> OneOf()
        {
            return Schemas;
        }
    }

    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static class SchemaBuilderExtensions
    {
        /// <summary>
        /// Creates schema copy with provided description.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="schemas">Schemas for oneOf.</param>
        /// <returns>New schema instance with provided description.</returns>
        public static TSchema OneOf<TSchema>(this TSchema source, params ISchema[] schemas)
            where TSchema : ICompositeBuilder<TSchema, IOneOf>, ISchema
        {
            return source.With(new OneOfComponent(schemas));
        }

        public static TComponent? GetComponent<TComponent>(this object source)
        {
            return source.GetSelfOrComponent<TComponent>();
        }
    }

    public interface IAllOf
    {
        public IEnumerable<ISchema> AllOf();
    }

    /// <summary>
    /// Static schema gets properties from static fields and properties.
    /// </summary>
    public interface IStaticSchema : IObjectSchemaProvider, IStaticPropertySet
    {
        /// <inheritdoc />
        IObjectSchema IObjectSchemaProvider.GetObjectSchema()
        {
            var properties = GetType().GetStaticProperties();
            return new MutableObjectSchema(name: GetType().Name, properties: properties);
        }
    }

    /// <summary>
    /// Static schema gets properties from static fields and properties.
    /// </summary>
    public class StaticSchema : IStaticSchema
    {
        /// <inheritdoc />
        public IObjectSchema GetObjectSchema()
        {
            var properties = GetType().GetStaticProperties();
            return new MutableObjectSchema(name: GetType().Name, properties: properties);
        }
    }
}
