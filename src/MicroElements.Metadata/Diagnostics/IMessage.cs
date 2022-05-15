// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Diagnostics
{
    // IError<TErrorCode>
    // Validation
    // Logging
    // ParseResult messages
    // Formatting messages

    // Use IPropertyContainer, Formatters, Parsers?
    // TODO: IReadOnlyList<KeyValuePair<string, object>> remove?
    // TODO: Immutability? see [Immutable] and ParseResult in NodaTime

    // TODO: Serilog uses: IEnumerable<KeyValuePair<string, object>>
    // TODO: NLog uses: IReadOnlyList<KeyValuePair<string, object>>

    /// <summary>
    /// Represents message.
    /// Can be used as simple log message, detailed or structured log message, validation message, diagnostic message.
    /// </summary>
    /// <para>
    /// Design notes:
    /// - <see cref="Timestamp"/>, <see cref="Severity"/> and <see cref="OriginalMessage"/> is minimal set to define message;
    /// - EventName can be used for grouping log messages, error codes;
    /// - IReadOnlyDictionary needed for structured logging systems;
    /// - Message should be serializable to cross process boundaries.
    /// </para>
    public interface IMessage : IReadOnlyList<KeyValuePair<string, object>>, IReadOnlyDictionary<string, object>
    {
        /// <summary>
        /// Gets date and time of message occurred.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets message severity.
        /// </summary>
        MessageSeverity Severity { get; }

        /// <summary>
        /// Gets original message.
        /// <para>OriginalMessage can be a pattern in form of MessageTemplates.org.</para>
        /// </summary>
        string OriginalMessage { get; }

        /// <summary>
        /// Gets event name.
        /// </summary>
        string? EventName { get; }

        /// <summary>
        /// Gets message properties.
        /// </summary>
        IReadOnlyCollection<KeyValuePair<string, object>> Properties { get; }

        /// <summary>
        /// Gets formatted message.
        /// <para>It's a result of MessageTemplate rendered with <seealso cref="Properties"/>.</para>
        /// </summary>
        string FormattedMessage { get; }
    }

    /// <summary>
    /// Message severity.
    /// </summary>
    public enum MessageSeverity
    {
        /// <summary>
        /// Information message.
        /// </summary>
        Information,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Error message.
        /// </summary>
        Error,
    }
}
