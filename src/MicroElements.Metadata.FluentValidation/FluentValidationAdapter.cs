// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using FluentValidation.Validators;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.FluentValidation
{
    public class FluentValidationAdapter
    {
        public void AddlyFluentValidatorToSchema(IPropertyValidator propertyValidator, ISchema property)
        {
            if (propertyValidator is INotNullValidator)
            {
                property.SetNotNull();
                property.SetRequired();
            }

            if (propertyValidator is INotEmptyValidator)
            {
                if (property.Type == typeof(string))
                    property.SetStringMinLength(1); // If not set more then 1

                if (property is ICollection)
                    property.SetStringMinLength(1);
            }

            if (propertyValidator is ILengthValidator lengthValidator)
            {
                if (lengthValidator.Max > 0)
                    property.SetStringMaxLength(lengthValidator.Max);

                if (lengthValidator.Min > 0)
                    property.SetStringMinLength(lengthValidator.Min);
            }

            if (propertyValidator is IRegularExpressionValidator regularExpressionValidator)
            {
                property.SetStringPattern(regularExpressionValidator.Expression);
            }

            if (propertyValidator.GetType().Name.Contains("EmailValidator"))
            {
                property.SetStringFormat("email");
            }

            if (propertyValidator is IComparisonValidator comparisonValidator)
            {

            }
        }
    }
}
