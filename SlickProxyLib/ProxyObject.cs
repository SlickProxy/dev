namespace SlickProxyLib
{
    using System.Collections.Generic;

    public class ProxyObject
    {
        public ProxyObject(OwinAppRequestInformation request)
        {
            this.BaseAddressWithScheme = request.BaseAddressWithScheme;
            this.Path = request.Path;
            this.Scheme = request.Scheme;
            this.Method = request.Method;
            this.Protocol = request.Protocol;
            this.QueryString = request.QueryString;
            this.HostName = request.HostName;
            this.HostNameWithPort = request.HostNameWithPort;
            this.RemoteIpAddress = request.RemoteIpAddress;
            this.RequestHeaders = request.RequestHeaders;
            this.Port = request.Port;
        }

        public string Scheme { get; internal set; }

        public string BaseAddressWithScheme { get; internal set; }

        public string Path { get; internal set; }

        public string Method { get; internal set; }

        public string Protocol { get; internal set; }

        public string QueryString { get; internal set; }

        /// <summary>
        ///     Port not implemented at the moment
        /// </summary>
        public string Port { get; internal set; }

        public string HostName { get; internal set; }

        public string HostNameWithPort { get; internal set; }

        public string RemoteIpAddress { get; internal set; }

        public IDictionary<string, string[]> RequestHeaders { get; internal set; }
    }
}