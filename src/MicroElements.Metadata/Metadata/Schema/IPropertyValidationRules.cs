// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Validation;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Property metadata that holds validation rules for property.
    /// </summary>
    public interface IPropertyValidationRules : IMetadata
    {
        /// <summary>
        /// Adds rule to property.
        /// </summary>
        /// <param name="validationRule">Rule to add.</param>
        void AddRule(IValidationRule validationRule);

        /// <summary>
        /// Gets rules.
        /// </summary>
        IReadOnlyCollection<IValidationRule> Rules { get; }
    }

    /// <summary>
    /// Property metadata that holds validation rules for property.
    /// </summary>
    public class PropertyValidationRules : IPropertyValidationRules
    {
        private readonly List<IValidationRule> _rules = new List<IValidationRule>();

        /// <inheritdoc/>
        public void AddRule(IValidationRule validationRule)
        {
            _rules.Add(validationRule);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IValidationRule> Rules => _rules;
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="IValidationRule{T}"/> to metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="validation">Property validation.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> AddValidation<T>(this IProperty<T> property, Func<IProperty<T>, IValidationRule<T>> validation)
        {
            property.AssertArgumentNotNull(nameof(property));
            validation.AssertArgumentNotNull(nameof(validation));

            return property.ConfigureMetadata<IProperty<T>, IPropertyValidationRules, PropertyValidationRules>(
                propertyValidation =>
                {
                    IValidationRule<T> validationRule = validation(property);
                    propertyValidation.AddRule(validationRule);
                });
        }

        /// <summary>
        /// Sets <see cref="IValidationRule{T}"/>.
        /// Replaces property metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="validation">Property validation.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetValidation<T>(this IProperty<T> property, Func<IProperty<T>, IValidationRule<T>> validation)
        {
            property.AssertArgumentNotNull(nameof(property));
            validation.AssertArgumentNotNull(nameof(validation));

            IValidationRule<T> validationRule = validation(property);
            PropertyValidationRules validationRules = new PropertyValidationRules();
            validationRules.AddRule(validationRule);

            return property.SetMetadata<IProperty<T>, IPropertyValidationRules>(validationRules);
        }

        /// <summary>
        /// Adds untyped validation to property metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="validationRule">Validation rule.</param>
        /// <returns>The same property.</returns>
        public static IProperty AddUntypedValidation(this IProperty property, IValidationRule validationRule)
        {
            property.AssertArgumentNotNull(nameof(property));
            validationRule.AssertArgumentNotNull(nameof(validationRule));

            return property.ConfigureMetadata<IProperty, IPropertyValidationRules, PropertyValidationRules>(
                validation => validation.AddRule(validationRule));
        }

        /// <summary>
        /// Gets validation rules attached to property.
        /// Rules are stored in <see cref="IPropertyValidationRules"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Validation rules.</returns>
        public static IEnumerable<IValidationRule> GetValidationRules(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));
            return property.GetMetadata<IPropertyValidationRules>()?.Rules ?? Array.Empty<IValidationRule>();
        }

        /// <summary>
        /// Gets validation rules attached to properties in schema.
        /// Rules are stored in <see cref="IPropertyValidationRules"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Validation rules.</returns>
        public static IEnumerable<IValidationRule> GetValidationRules(this IPropertySet schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetProperties().SelectMany(property => property.GetValidationRules());
        }
    }
}
