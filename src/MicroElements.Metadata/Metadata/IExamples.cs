// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has example objects.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IExamples<out T> : IMetadata
    {
        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyCollection<T> Examples { get; }
    }

    /// <summary>
    /// Holds value example list.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class ExampleList<T> : IExamples<T>
    {
        /// <inheritdoc />
        public IReadOnlyCollection<T> Examples { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleList{T}"/> class.
        /// </summary>
        /// <param name="examples">Value examples.</param>
        public ExampleList(IReadOnlyCollection<T> examples)
        {
            Examples = examples.AssertArgumentNotNull(nameof(examples));
        }
    }
}
