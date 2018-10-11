namespace SlickProxyLib
{
    using System.Net;
    using System.Net.Http;

    public class ResponseInspection
    {
        public ResponseInspection(string from, string to, HttpStatusCode statusCode, string contentType, HttpContent httpContent)
        {
            this.From = from;
            this.To = to;
            this.StatusCode = statusCode;
            this.ContentType = contentType;
            this.HttpContent = httpContent;
        }

        public string From { get; }

        public string To { get; }

        public HttpStatusCode StatusCode { get; }

        public string ContentType { get; }

        public HttpContent HttpContent { get; }
    }
}