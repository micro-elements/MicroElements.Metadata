// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation result is a wrapper for data that was validated and its validation errors (if any).
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets data that was validated.
        /// </summary>
        public IPropertyContainer Data { get; }

        /// <summary>
        /// Gets a value indicating whether data is valid (no validation messages provided).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets validation messages.
        /// </summary>
        public IReadOnlyCollection<Message> ValidationMessages { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="data">Data that was validated.</param>
        /// <param name="validationMessages">Validation messages.</param>
        public ValidationResult(IPropertyContainer data, IEnumerable<Message> validationMessages)
        {
            Data = data.AssertArgumentNotNull(nameof(data));
            ValidationMessages = validationMessages?.ToArray() ?? Array.Empty<Message>();
            IsValid = ValidationMessages.Count == 0;
        }
    }
}
