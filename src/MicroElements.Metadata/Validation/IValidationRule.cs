// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation rule.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Validates property or other aspect of <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer"><see cref="IPropertyContainer"/> to validate.</param>
        /// <returns>Validation messages.</returns>
        IEnumerable<Message> Validate(IPropertyContainer propertyContainer);
    }

    /// <summary>
    /// Validation rule with ability to configure validation message.
    /// </summary>
    public interface IConfigurableValidationRule : IValidationRule
    {
        /// <summary>
        /// Configures validation message.
        /// </summary>
        /// <param name="configureMessage">Configuration function.</param>
        void ConfigureMessage(Func<Message, Message> configureMessage);
    }
}
