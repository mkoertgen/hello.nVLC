using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace MediaPlayer
{
    public static class UriExtensions
    {
        public static string GetFileName(this Uri uri, bool verifyAccess)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            if (uri.IsFile)
            {
                var fileName = uri.LocalPath;
                if (verifyAccess) VerifyFileExists(fileName);
                return fileName;
            }
            return string.Empty;
        }

        public static void VerifyUriExists(this Uri uri, TimeSpan timeOut)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            VerifyUriExists(uri, (int)timeOut.TotalMilliseconds);
        }

        public static void VerifyUriExists(this Uri uri, int timeout = 15000)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            if (uri.IsFile)
                VerifyFileExists(uri.LocalPath);
            else
                VerifyRemoteUriExists(uri, timeout);
        }

        private static void VerifyRemoteUriExists(Uri uri, int timeout)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                var request = WebRequest.Create(uri);
                request.Method = "HEAD";
                request.Timeout = timeout;

                using (var response = request.GetResponse())
                {
                    var httpWebResponse = response as HttpWebResponse;
                    if (httpWebResponse != null)
                    {
                        VerifyHttpStatusOk(uri, httpWebResponse.StatusCode);
                        return;
                    }

                    // TODO: other casts/checks?
                }
            }
            catch (NotSupportedException)
            {
                Trace.TraceWarning("Could not pre-access check due to unknown uri scheme: \"{0}\"", uri.Scheme);
            }
        }

        private static void VerifyFileExists(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File does not exist", filename);
        }

        private static void VerifyHttpStatusOk(Uri uri, HttpStatusCode statusCode)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            switch (statusCode)
            {
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Continue:
                case HttpStatusCode.Found:
                case HttpStatusCode.OK:
                    return;
                default:
                    throw new UriNotFoundException(uri, statusCode);
            }
        }
    }
}