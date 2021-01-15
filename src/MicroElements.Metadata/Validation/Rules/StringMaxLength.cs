// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    public class MaxLengthValidationRule : BasePropertyRule<string>
    {
        private readonly IStringMaxLength? _maxLength;

        public MaxLengthValidationRule(IProperty<string> property, IStringMaxLength? maxLength = null)
            : base(property)
        {
            _maxLength = maxLength ?? property.GetMetadata<IStringMaxLength>();
            if (_maxLength != null && _maxLength.MaxLength.HasValue)
            {
                this.SetDefaultMessageFormat("value '{value}' is too long (length: {length}, maxLength: {maxLength})");
                this.ConfigureMessage((message, value, pc) =>
                    message
                        .WithProperty("length", GetLength(value.ValueUntyped as string))
                        .WithProperty("maxLength", _maxLength.MaxLength.Value));
            }
        }

        /// <inheritdoc />
        protected override bool IsValid(string? value, IPropertyContainer propertyContainer)
        {
            if (_maxLength != null && _maxLength.MaxLength.HasValue)
            {
                int valueLength = GetLength(value);
                return valueLength <= _maxLength.MaxLength;
            }

            return true;
        }

        private static int GetLength(string? value)
        {
            return value?.Length ?? 0;
        }
    }
}
