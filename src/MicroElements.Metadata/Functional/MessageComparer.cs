// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Functional
{
    /// <summary>
    /// Message equality comparer.
    /// </summary>
    public class MessageComparer : IEqualityComparer<Message>
    {
        [Flags]
        public enum CompareFlags
        {
            FormattedMessage = 1 << 0,
            OriginalMessage = 1 << 1,
            EventName = 1 << 2,
            Severity = 1 << 3,
            Properties = 1 << 4,
        }

        private readonly CompareFlags _compareFlags;

        /// <summary>
        /// Gets message comparer by <see cref="Message.FormattedMessage"/>.
        /// </summary>
        public static IEqualityComparer<Message> ByFormattedMessage { get; } = new MessageComparer(CompareFlags.FormattedMessage);

        /// <summary>
        /// Gets message comparer by <see cref="Message.OriginalMessage"/>.
        /// </summary>
        public static IEqualityComparer<Message> ByOriginalMessage { get; } = new MessageComparer(CompareFlags.OriginalMessage);

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageComparer"/> class.
        /// </summary>
        /// <param name="compareFlags">Indicates message parts that should be used for comparison.</param>
        public MessageComparer(CompareFlags compareFlags)
        {
            if (compareFlags == 0)
                compareFlags = CompareFlags.FormattedMessage;
            _compareFlags = compareFlags;
        }

        /// <inheritdoc />
        public bool Equals(Message? x, Message? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;

            bool isEqual = true;
            bool isSingleFlag = false;

            bool HasFlag(CompareFlags flags)
            {
                if (isSingleFlag)
                {
                    // already checked as single flag so no need to check other variants.
                    return false;
                }

                if (_compareFlags == flags)
                {
                    isSingleFlag = true;
                    return true;
                }

                bool hasFlag = _compareFlags.HasFlag(flags);
                isSingleFlag = false;
                return hasFlag;
            }

            if (HasFlag(CompareFlags.FormattedMessage))
                isEqual = isEqual && x.FormattedMessage == y.FormattedMessage;

            if (HasFlag(CompareFlags.OriginalMessage))
                isEqual = isEqual && x.OriginalMessage == y.OriginalMessage;

            if (HasFlag(CompareFlags.EventName))
                isEqual = isEqual && x.EventName == y.EventName;

            if (HasFlag(CompareFlags.Severity))
                isEqual = isEqual && x.Severity == y.Severity;

            if (HasFlag(CompareFlags.Properties))
                isEqual = isEqual && PropertiesFormatted(x.Properties) == PropertiesFormatted(y.Properties);

            return isEqual;
        }

        /// <inheritdoc />
        public int GetHashCode(Message message)
        {
            bool isSingleFlag = false;

            bool HasFlag(CompareFlags flags)
            {
                if (isSingleFlag)
                {
                    // already checked as single flag so no need to check other variants.
                    return false;
                }

                if (_compareFlags == flags)
                {
                    isSingleFlag = true;
                    return true;
                }

                bool hasFlag = _compareFlags.HasFlag(flags);
                isSingleFlag = false;
                return hasFlag;
            }

            int hash = 0;

            if (HasFlag(CompareFlags.FormattedMessage))
                hash = HashCode.Combine(hash, message.FormattedMessage);

            if (HasFlag(CompareFlags.OriginalMessage))
                hash = HashCode.Combine(hash, message.OriginalMessage);

            if (HasFlag(CompareFlags.EventName))
                hash = HashCode.Combine(hash, message.EventName);

            if (HasFlag(CompareFlags.Severity))
                hash = HashCode.Combine(hash, message.Severity);

            if (HasFlag(CompareFlags.Properties))
                hash = HashCode.Combine(hash, PropertiesFormatted(message.Properties));

            return hash;
        }

        private static string PropertiesFormatted(IReadOnlyCollection<KeyValuePair<string, object>> properties)
        {
            return properties.OrderBy(pair => pair.Key).FormatAsTuple();
        }
    }
}
