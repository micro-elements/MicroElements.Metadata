// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Default implementation of validation rule provider.
    /// It creates validation rules from property metadata.
    /// </summary>
    public class ValidationProvider : IValidationProvider
    {
        /// <summary>
        /// Static instance can be used because provider is stateless.
        /// </summary>
        public static ValidationProvider Instance { get; } = new ValidationProvider();

        /// <inheritdoc />
        public IEnumerable<IValidationRule> GetValidationRules(IProperty property)
        {
            IEnumerable<IMetadata> metadataObjects = property
                .GetInstanceMetadata(autoCreate: false)
                .Select(propertyValue => propertyValue.ValueUntyped)
                .OfType<IMetadata>();

            foreach (IMetadata metadata in metadataObjects)
            {
                if (metadata is INullability nullability)
                {
                    yield return new ShouldMatchNullability(property, nullability);
                }
                else if (metadata is IStringMinLength minLength)
                {
                    yield return new StringMinLengthValidationRule((IProperty<string>)property, minLength);
                }
                else if (metadata is IStringMaxLength maxLength)
                {
                    yield return new StringMaxLengthValidationRule((IProperty<string>)property, maxLength);
                }
                else if (metadata is IAllowedValues allowedValues)
                {
                    IValidationRule? validationRule = CreateRuleUntyped(property, allowedValues);
                    if (validationRule != null)
                        yield return validationRule;
                }
                else if (metadata is INumericInterval numericInterval)
                {
                    IValidationRule? validationRule = CreateRuleUntyped(property, numericInterval);
                    if (validationRule != null)
                        yield return validationRule;
                }
                else if (metadata is IPropertyValidationRules propertyValidationRules)
                {
                    foreach (var rule in propertyValidationRules.Rules)
                    {
                        yield return rule;
                    }
                }
            }
        }

        private IValidationRule? CreateRuleUntyped(IProperty property, IMetadata validationMetadata)
        {
            MethodInfo? genericMethod = GetType().GetMethod(nameof(CreateRule), BindingFlags.Instance | BindingFlags.NonPublic)?.MakeGenericMethod(property.Type);
            return genericMethod?.Invoke(this, new object[] {property, validationMetadata}) as IValidationRule;
        }

        private IPropertyValidationRule<T>? CreateRule<T>(IProperty<T> property, IMetadata validationMetadata)
        {
            return validationMetadata switch
            {
                IAllowedValues allowedValues => new OnlyAllowedValuesRule<T>(property, (IAllowedValues<T>?)allowedValues),
                INumericInterval numericInterval => new ShouldBeInInterval<T>(property, numericInterval),
                _ => null,
            };
        }
    }
}
