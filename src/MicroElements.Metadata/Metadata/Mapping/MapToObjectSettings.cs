using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Mapping
{
    public class MapToObjectSettings<TModel>
    {
        public Func<TModel>? Factory { get; init; }

        public Func<IProperty, bool>? SourceFilter { get; init; }

        public Func<IProperty, string>? TargetName { get; init; }

        /// <summary>
        /// Gets or sets action that will be invoked on mapping or validation error.
        /// </summary>
        public Action<Message>? LogMessage { get; set; }
    }
}
