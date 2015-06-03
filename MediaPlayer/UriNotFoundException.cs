using System;
using System.Net;

namespace MediaPlayer
{
    public sealed class UriNotFoundException : MediaException
    {
        public Uri Uri { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public UriNotFoundException(Uri uri, HttpStatusCode statusCode) :
            base($"Could not get uri \"{uri}\" (HTTP status: {statusCode})")
        {
            Uri = uri;
            StatusCode = statusCode;
        }
    }
}