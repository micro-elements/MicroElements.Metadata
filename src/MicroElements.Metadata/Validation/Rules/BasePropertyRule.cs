// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation.Rules
{
    /// <summary>
    /// Base configurable property validation rule.
    /// Implement <see cref="IsValid"/> and <see cref="GetMessage"/>.
    /// You can use {propertyName} and {value} in message format.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public abstract class BasePropertyRule<T> : IConfigurableValidationRule
    {
        // Message configuration.
        private List<Func<Message, Message>> _configureMessage;

        /// <summary>
        /// Gets the property to validate.
        /// </summary>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePropertyRule{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        protected BasePropertyRule(IProperty<T> property)
        {
            Property = property;
        }

        /// <inheritdoc/>
        public IEnumerable<Message> Validate(IPropertyContainer propertyContainer)
        {
            T value = propertyContainer.GetValue(Property);
            if (!IsValid(value, propertyContainer))
                yield return GetConfiguredMessage(value, propertyContainer);
        }

        /// <inheritdoc/>
        public void ConfigureMessage(Func<Message, Message> configureMessage)
        {
            _configureMessage ??= new List<Func<Message, Message>>();
            _configureMessage.Add(configureMessage);
        }

        /// <summary>
        /// Checks that property value is valid.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <returns>True if value is valid.</returns>
        protected abstract bool IsValid(T value, IPropertyContainer propertyContainer);

        /// <summary>
        /// Gets validation message if property value in not valid.
        /// You can use {propertyName} and {value} in message format.
        /// </summary>
        /// <param name="value">Value that is not valid according <see cref="IsValid"/> condition.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <returns>Validation message that can be configured in <see cref="ConfigureMessage"/>.</returns>
        protected abstract Message GetMessage(T value, IPropertyContainer propertyContainer);

        /// <summary>
        /// Gets configured message.
        /// 1. Gets message by <see cref="GetMessage"/>
        /// 2. Adds property values {propertyName} and {value} for using in templated messages.
        /// 3. Applies ConfigureMessage chain.
        /// </summary>
        /// <param name="value">Value that is not valid according <see cref="IsValid"/> condition.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <returns>Configured message.</returns>
        protected virtual Message GetConfiguredMessage(T value, IPropertyContainer propertyContainer)
        {
            Message message = GetMessage(value, propertyContainer)
                .WithProperty("propertyName", Property.Name)
                .WithProperty("value", value);

            if (_configureMessage != null)
            {
                foreach (var configureMessage in _configureMessage)
                {
                    message = configureMessage(message);
                }
            }

            return message;
        }
    }
}
