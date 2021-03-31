using System.Collections.Generic;

namespace MicroElements.Metadata
{
    public interface IPropertyContainer<TSchema> : IPropertyContainer, IKnownPropertySet<TSchema>, IPropertySet
        where TSchema : IPropertySet, new()
    {
        TSchema Schema { get; }

        /// <inheritdoc />
        IEnumerable<IProperty> IPropertySet.GetProperties() => Schema.GetProperties();
    }
}
