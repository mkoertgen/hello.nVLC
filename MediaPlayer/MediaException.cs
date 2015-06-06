using System;

namespace MediaPlayer
{
    public class MediaException : Exception
    {
        public MediaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MediaException(string message) : base(message)
        {
        }

        public MediaException()
        {
        }
    }
}