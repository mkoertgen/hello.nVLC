using System;
using System.Net;

namespace MediaPlayer
{
    public sealed class UriNotFoundException : MediaException
    {
        public UriNotFoundException(Uri uri, HttpStatusCode statusCode) :
            base($"Could not get uri \"{uri}\" (HTTP status: {statusCode})")
        {
            Uri = uri;
            StatusCode = statusCode;
        }

        public Uri Uri { get; }
        public HttpStatusCode StatusCode { get; }
    }
}