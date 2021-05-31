// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Validation rule based on property metadata <see cref="IStringMaxLength"/>.
    /// Checks that string length aligns with the maximum allowed length.
    /// </summary>
    public class StringMaxLengthValidationRule : PropertyValidationRule<string>
    {
        private readonly IStringMaxLength? _maxLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMaxLengthValidationRule"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="maxLength">Maximum allowed length for string values.</param>
        public StringMaxLengthValidationRule(IProperty<string> property, IStringMaxLength? maxLength = null)
            : base(property)
        {
            _maxLength = maxLength ?? property.GetSchemaMetadata<IStringMaxLength>();
            if (_maxLength != null)
            {
                this.SetDefaultMessageFormat("Value '{value}' is too long (length: {length}, maxLength: {maxLength})");
                this.ConfigureMessage((message, value, pc) =>
                    message
                        .WithProperty("length", GetLength(value.ValueUntyped as string))
                        .WithProperty("maxLength", _maxLength.MaxLength));
            }
        }

        /// <inheritdoc />
        protected override bool IsValid(string? value)
        {
            if (_maxLength != null)
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
