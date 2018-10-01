namespace SlickProxyLib
{
    using System;
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
            this.QueryStringWithPrefix = request.QueryStringWithPrefix;
            this.ExtensionlessWithExtension = extension =>
            {
                if (request.IsMatched)
                    return null;
                return $"{request.BaseAddressWithScheme}{request.Path}.{extension}{request.QueryStringWithPrefix}";
            };
            this.ExtensionlessWithExtensionAndDomain = (extension, baseAddressWithScheme) =>
            {
                if (request.IsMatched)
                    return null;
                return $"{baseAddressWithScheme}{request.Path}.{extension}{request.QueryStringWithPrefix}";
            };
            this.RespondWithString = s =>
            {
                if (request.IsMatched)
                    return;
                this.ReturnString = s;
            };
            this.RedirectTo = s =>
            {
                if (request.IsMatched)
                    return;
                this.RedirectToString = s;
            };
            this.RespondWithObjectAsJson = s =>
            {
                if (request.IsMatched)
                    return;
                this.ReturnObjectAsJson = s;
            };
            this.ForwardToDomain = s =>
            {
                if (request.IsMatched)
                    return;
                this.Forwarding = s;
            };
            this.BaseAddressWithoutScheme = request.BaseAddressWithoutScheme;
            this.UriWithoutScheme = request.UriWithoutScheme;
            this.PathAndQuery = request.PathAndQuery;
        }

        public string UseReferer(string referer,string route)
        {
            Referer = referer;
            return route;
        }


        internal string Referer { get; set; }
        public string PathAndQuery { get; internal set; }

        public string BaseAddressWithoutScheme { get; internal set; }

        public string UriWithoutScheme { get; internal set; }

        internal object ReturnObjectAsJson { get; set; }

        internal string Forwarding { get; set; }

        internal string ReturnString { set; get; }

        internal string RedirectToString { set; get; }

        public Func<string, string> ExtensionlessWithExtension { get; internal set; }

        public Action<string> RespondWithString { get; internal set; }

        public Action<string> ForwardToDomain { get; internal set; }

        public Action<object> RespondWithObjectAsJson { get; internal set; }

        public Action<string> RedirectTo { get; internal set; }

        public Func<string, string, string> ExtensionlessWithExtensionAndDomain { get; internal set; }

        public string QueryStringWithPrefix { get; internal set; }

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

        public string AsHttps => $"https://{this.UriWithoutScheme}";

        public string AsHttp => $"http://{this.UriWithoutScheme}";
    }
}