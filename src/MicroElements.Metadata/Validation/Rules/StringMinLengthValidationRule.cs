// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Validation rule based on property metadata <see cref="IStringMinLength"/>.
    /// Checks that string length aligns with the minimum allowed length.
    /// </summary>
    public class StringMinLengthValidationRule : BasePropertyRule<string>
    {
        private readonly IStringMinLength? _minLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMinLengthValidationRule"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="minLength">Minimum allowed length for string values.</param>
        public StringMinLengthValidationRule(IProperty<string> property, IStringMinLength? minLength = null)
            : base(property)
        {
            _minLength = minLength ?? property.GetMetadata<IStringMinLength>();
            if (_minLength != null && _minLength.MinLength.HasValue)
            {
                this.SetDefaultMessageFormat("Value '{value}' is too short (length: {length}, minLength: {minLength})");
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
