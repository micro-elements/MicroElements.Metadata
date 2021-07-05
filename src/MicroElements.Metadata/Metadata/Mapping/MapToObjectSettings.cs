// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Mapping
{
    public interface IMapToObjectSettings
    {
        Func<IProperty, bool>? SourceFilter { get; }

        Func<IProperty, string>? TargetName { get; }

        /// <summary>
        /// Gets or sets action that will be invoked on mapping or validation error.
        /// </summary>
        Action<Message>? LogMessage { get; }
    }

    public record MapToObjectSettings<TModel> : IMapToObjectSettings
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
