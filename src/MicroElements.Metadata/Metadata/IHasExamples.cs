// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has example objects.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IHasExamples<out T>
    {
        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyCollection<T> Examples { get; }
    }
}
