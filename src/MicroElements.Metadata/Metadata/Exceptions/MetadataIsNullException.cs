using System;
using System.Runtime.Serialization;

namespace MicroElements.Metadata.Exceptions
{
    [Serializable]
    public class MetadataIsNullException : Exception
    {
        /// <inheritdoc />
        public MetadataIsNullException()
        {
        }

        /// <inheritdoc />
        protected MetadataIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public MetadataIsNullException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public MetadataIsNullException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
