// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Represents validation rule provider.
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

    /// <summary>
    /// Default <see cref="IValidationProvider"/> implementation.
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
                if (metadata is IStringMinLength minLength)
                {
                    yield return new StringMinLengthValidationRule((IProperty<string>)property, minLength);
                }
                else if (metadata is IStringMaxLength maxLength)
                {
                    yield return new StringMaxLengthValidationRule((IProperty<string>)property, maxLength);
                }
                else if (metadata is IAllowedValues allowedValues)
                {
                    //yield return new OnlyAllowedValuesRule<>();
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
    }
}
