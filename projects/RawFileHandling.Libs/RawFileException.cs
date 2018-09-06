using System;
using System.Runtime.Serialization;

namespace RawFileHandling.Libs
{
    [Serializable]
    internal class RawFileException : Exception
    {
        public RawFileException()
        {
        }

        public RawFileException(string message)
            : base(message)
        {
        }

        public RawFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RawFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}