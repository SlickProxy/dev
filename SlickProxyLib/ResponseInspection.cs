namespace SlickProxyLib
{
    using System.IO;
    using System.Net;
    using System.Net.Http;

    public class ResponseInspection
    {
        public ResponseInspection(string @from, string to,  HttpStatusCode statusCode, string contentType, HttpContent httpContent)
        {
            this.From = @from;
            this.To = to;
            this.StatusCode = statusCode;
            this.ContentType = contentType;
            this.HttpContent = httpContent;
        }

        public string From { private set; get; }

        public string To { private set; get; }
        public HttpStatusCode StatusCode { private set; get; }
        public string ContentType { private set; get; }
        public HttpContent HttpContent { private set; get; }
    }
}