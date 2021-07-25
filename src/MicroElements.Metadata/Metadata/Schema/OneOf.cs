using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    public class OneOf : ISchemaComponent
    {
        public IReadOnlyCollection<ISchema> Schemas { get; }
    }
}
