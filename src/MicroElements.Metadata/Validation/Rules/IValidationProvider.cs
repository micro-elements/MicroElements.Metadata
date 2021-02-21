// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Represents validation rule provider.
    /// It creates validation rules from property metadata.
    /// </summary>
    public interface IValidationProvider
    {
        /// <summary>
        /// Gets validation rules for property.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Validation rules for property.</returns>
        IEnumerable<IValidationRule> GetValidationRules(IProperty property);
    }
}
