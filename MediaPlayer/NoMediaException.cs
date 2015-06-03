using System;

namespace MediaPlayer
{
    public sealed class NoMediaException : MediaException
    {
        public NoMediaException(string message, Exception innerException) : base(message, innerException)
        {}

        public NoMediaException(string message) : base(message)
        {}

        public NoMediaException()
        {}
    }
}