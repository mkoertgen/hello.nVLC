using System;

namespace Hello.nVLC.Media
{
    public sealed class MediaNotFoundException : MediaException
    {
        public MediaNotFoundException(string message, Exception innerException) : base(message, innerException)
        {}

        public MediaNotFoundException(string message) : base(message)
        {}

        public MediaNotFoundException()
        {}
    }
}