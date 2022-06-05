// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace MicroElements.Metadata.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        /// <inheritdoc />
        public NotFoundException()
        {
        }

        /// <inheritdoc />
        protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public NotFoundException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
