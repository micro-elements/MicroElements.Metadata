﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Validation rule linker.
    /// </summary>
    /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
    public interface IValidationRuleLinker<out TValidationRule>
        where TValidationRule : IValidationRule
    {
        /// <summary>
        /// Gets the first rule.
        /// </summary>
        IValidationRule FirstRule { get; }

        /// <summary>
        /// Combines with next rule.
        /// </summary>
        /// <param name="nextRule">Next rule.</param>
        /// <returns>New combined validation rule.</returns>
        TValidationRule CombineWith(IValidationRule nextRule);
    }

    /// <summary>
    /// Validation rule linker.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
    public interface IValidationRuleLinker<T, out TValidationRule>
        where TValidationRule : IPropertyValidationRule<T>
    {
        /// <summary>
        /// Gets the first rule.
        /// </summary>
        IPropertyValidationRule<T> FirstRule { get; }

        /// <summary>
        /// Combines with next rule.
        /// </summary>
        /// <param name="nextRule">Next rule.</param>
        /// <returns>New combined validation rule.</returns>
        TValidationRule CombineWith(IPropertyValidationRule<T> nextRule);
    }
}
