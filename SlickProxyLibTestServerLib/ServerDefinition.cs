namespace SlickProxyLibTestServerLib
{
    using System.Net;
    using System.Net.Http;

    public class ServerDefinition
    {
        internal ServerDefinition(HttpMethod method, object response = null, object argument = null, HttpStatusCode responseStatusCode = HttpStatusCode.OK)
        {
            this.Method = method;
            this.Argument = argument;
            this.Response = response;
            this.ResponseStatusCode = responseStatusCode;
        }

        public object Response { get; internal set; }

        public HttpStatusCode ResponseStatusCode { get; internal set; }

        public HttpMethod Method { get; internal set; }

        public object Argument { get; internal set; }
    }
}