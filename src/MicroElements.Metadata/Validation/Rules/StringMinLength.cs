// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    public class MinLengthValidationRule : BasePropertyRule<string>
    {
        private readonly IStringMinLength? _minLength;

        public MinLengthValidationRule(IProperty<string> property, IStringMinLength? minLength = null)
            : base(property)
        {
            _minLength = minLength ?? property.GetMetadata<IStringMinLength>();
            if (_minLength != null && _minLength.MinLength.HasValue)
            {
                this.SetDefaultMessageFormat("value '{value}' is too short (length: {length}, minLength: {minLength})");
                this.ConfigureMessage((message, value, pc) =>
                    message
                        .WithProperty("length", GetLength(value.ValueUntyped as string))
                        .WithProperty("minLength", _minLength.MinLength.Value));
            }
        }

        /// <inheritdoc />
        protected override bool IsValid(string? value, IPropertyContainer propertyContainer)
        {
            if (_minLength != null && _minLength.MinLength.HasValue)
            {
                int valueLength = GetLength(value);
                return valueLength >= _minLength.MinLength;
            }

            return true;
        }

        private static int GetLength(string? value)
        {
            return value?.Length ?? 0;
        }
    }
}
