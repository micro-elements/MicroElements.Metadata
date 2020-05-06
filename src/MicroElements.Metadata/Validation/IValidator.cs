// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Validation
{
    /// <summary>
    /// Represents validator based on defined metadata.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Gets validation rules.
        /// </summary>
        /// <returns>Validation rules.</returns>
        IEnumerable<IValidationRule> GetRules();
    }
}
