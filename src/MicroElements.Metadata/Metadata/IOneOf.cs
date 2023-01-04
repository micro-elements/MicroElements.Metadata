using System.Collections.Generic;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
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
}
