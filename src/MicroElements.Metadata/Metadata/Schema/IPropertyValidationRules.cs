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
        /// Gets the property.
        /// </summary>
        IProperty Property { get; }

        /// <summary>
        /// Gets property validation rules.
        /// </summary>
        IReadOnlyCollection<IValidationRule> Rules { get; }
    }

    /// <summary>
    /// Property metadata that holds validation rules for property.
    /// </summary>
    public class PropertyValidationRules : IPropertyValidationRules
    {
        /// <inheritdoc />
        public IProperty Property { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IValidationRule> Rules { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationRules"/> class.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="validationRules">Validation rules for property.</param>
        public PropertyValidationRules(IProperty property, IReadOnlyCollection<IValidationRule>? validationRules = null)
        {
            Property = property.AssertArgumentNotNull(nameof(property));
            Rules = validationRules ?? Array.Empty<IValidationRule>();
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="IPropertyValidationRule{T}"/> to metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="validation">Property validation.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> AddValidation<T>(this IProperty<T> property, Func<IProperty<T>, IPropertyValidationRule<T>> validation)
        {
            property.AssertArgumentNotNull(nameof(property));
            validation.AssertArgumentNotNull(nameof(validation));

            return property.ConfigureMetadata<IProperty<T>, IPropertyValidationRules>(
                createMetadata: CreatePropertyValidationRules,
                configureMetadata: propertyValidation =>
                {
                    IPropertyValidationRule<T> validationRule = validation(property);
                    return propertyValidation.AddRule(validationRule);
                });
        }

        /// <summary>
        /// Adds untyped validation to property metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="validationRule">Validation rule.</param>
        /// <returns>The same property.</returns>
        public static IProperty AddValidation(this IProperty property, IValidationRule validationRule)
        {
            property.AssertArgumentNotNull(nameof(property));
            validationRule.AssertArgumentNotNull(nameof(validationRule));

            return property.ConfigureMetadata<IProperty, IPropertyValidationRules>(
                createMetadata: CreatePropertyValidationRules,
                configureMetadata: propertyValidation => propertyValidation.AddRule(validationRule));
        }

        /// <summary>
        /// Sets <see cref="IPropertyValidationRule{T}"/>.
        /// Replaces property metadata <see cref="IPropertyValidationRules"/>.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="validation">Property validation.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetValidation<T>(this IProperty<T> property, Func<IProperty<T>, IPropertyValidationRule<T>> validation)
        {
            property.AssertArgumentNotNull(nameof(property));
            validation.AssertArgumentNotNull(nameof(validation));

            IPropertyValidationRule<T> validationRule = validation(property);
            IPropertyValidationRules validationRules = new PropertyValidationRules(property, new[] { validationRule });

            return property.SetMetadata<IProperty<T>, IPropertyValidationRules>(validationRules);
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
            return property.GetSchemaMetadata<IPropertyValidationRules>()?.Rules ?? Array.Empty<IValidationRule>();
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

        /// <summary>
        /// Gets validation rules attached to properties in schema.
        /// Rules are stored in <see cref="IPropertyValidationRules"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Validation rules.</returns>
        public static IEnumerable<IValidationRule> GetValidationRules(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetProperties().SelectMany(property => property.GetValidationRules());
        }

        private static IPropertyValidationRules CreatePropertyValidationRules(IProperty property)
        {
            return new PropertyValidationRules(property);
        }

        private static IPropertyValidationRules AddRule(this IPropertyValidationRules propertyValidationRules, IValidationRule validationRule)
        {
            return new PropertyValidationRules(propertyValidationRules.Property, propertyValidationRules.Rules.Append(validationRule).ToArray());
        }
    }
}
