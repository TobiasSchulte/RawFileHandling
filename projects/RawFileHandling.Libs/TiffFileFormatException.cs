using System;

namespace RawFileHandling.Libs
{
    public class TiffFileFormatException : Exception
    {
        public TiffFileFormatException(string message)
            : base(message)
        {
        }
    }
}
