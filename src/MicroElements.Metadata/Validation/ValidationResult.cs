// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation result is a wrapper for data that was validated and its validation errors (if any).
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    public readonly struct ValidationResult<T>
    {
        /// <summary>
        /// Gets data that was validated.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets validation messages.
        /// </summary>
        public IReadOnlyCollection<Message> ValidationMessages { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult{T}"/> struct.
        /// </summary>
        /// <param name="data">Data that was validated.</param>
        /// <param name="validationMessages">Validation messages.</param>
        public ValidationResult(T data, IReadOnlyCollection<Message>? validationMessages)
        {
            Data = data.AssertArgumentNotNull(nameof(data));
            ValidationMessages = validationMessages ?? Array.Empty<Message>();
        }

        /// <summary>
        /// Validation result can be implicitly cast to its Data type.
        /// </summary>
        /// <param name="validationResult">Source validation result.</param>
        public static implicit operator T(ValidationResult<T> validationResult) => validationResult.Data;
    }

    /// <summary>
    /// Extension methods for <see cref="ValidationResult{T}"/>.
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Gets a value indicating whether data is valid (no validation messages provided).
        /// Treats all messages as errors. Use <see cref="HasErrors{T}"/> for error check.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="validationResult">Source validation result.</param>
        /// <returns>True if result is valid.</returns>
        public static bool IsValid<T>(this ValidationResult<T> validationResult) =>
            validationResult.ValidationMessages.Count == 0;

        /// <summary>
        /// Gets a value indicating whether result has errors.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="validationResult">Source validation result.</param>
        /// <returns>True if result has errors.</returns>
        public static bool HasErrors<T>(this ValidationResult<T> validationResult) =>
            validationResult.ValidationMessages.Any(message => message.Severity == MessageSeverity.Error);
    }
}
