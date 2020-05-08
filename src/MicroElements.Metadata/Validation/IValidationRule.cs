// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation rule.
    /// </summary>
    public interface IValidationRule : IMetadataProvider
    {
        /// <summary>
        /// Validates property or other aspect of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        IEnumerable<Message> Validate(IPropertyContainer propertyContainer);
    }

    /// <summary>
    /// Typed validation rule.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IValidationRule<T> : IValidationRule
    {
        /// <summary>
        /// Gets property to validate.
        /// </summary>
        IProperty<T> Property { get; }
    }
}
