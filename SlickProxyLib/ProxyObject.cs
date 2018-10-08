namespace SlickProxyLib
{
    using Microsoft.Owin;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class ProxyObject
    {
        private readonly OwinAppRequestInformation Request;
        internal HttpStatusCode DefaultDenyStatusCode = HttpStatusCode.NotFound;

        public ProxyObject(OwinAppRequestInformation request, IOwinContext context)
        {
            this.Request = request;
            this.Context = context;
            this.QueryParams = request.QueryParams;
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

            this.SetResponseContentType = ct => { request.ResponseContentHeadersContentType = ct; };

            this.ExtensionlessWithExtension = extension =>
            {
                if (request.IsMatched)
                    return null;

                return $"{request.BaseAddressWithScheme}{(request.Path == "/" ? "/index" : request.Path)}.{extension}{request.QueryStringWithPrefix}";
            };
            this.ExtensionlessWithExtensionAndDomain = (extension, baseAddressWithScheme) =>
            {
                if (request.IsMatched)
                    return null;
                return $"{baseAddressWithScheme}{(request.Path == "/" ? "/index" : request.Path)}.{extension}{request.QueryStringWithPrefix}";
            };
            this.RespondWithString = s =>
            {
                if (request.IsMatched)
                    return null;
                this.ReturnString = s;
                return null;
            };
            this.RedirectTo = s =>
            {
                if (request.IsMatched)
                    return null;
                this.RedirectToString = s;
                return null;
            };
            this.RespondWithObjectAsJson = s =>
            {
                if (request.IsMatched)
                    return null;
                this.ReturnObjectAsJson = s;
                return null;
            };
            this.ForwardToDomain = s =>
            {
                if (request.IsMatched)
                    return null;
                this.Forwarding = s;
                return null;
            };
            this.DenyWith = (code, message) =>
            {
                if (request.IsMatched)
                    return null;
                this.BlockRequestWith = new Tuple<HttpStatusCode, string>(code, message);
                return null;
            };
            this.Deny = () =>
            {
                if (request.IsMatched)
                    return null;
                this.BlockRequestWith = new Tuple<HttpStatusCode, string>(this.DefaultDenyStatusCode, null);
                return null;
            };
            this.Allow = () =>
            {
                if (request.IsMatched)
                    return null;

                this.ContinueToOtherOwinPipeline = true;

                return null;
            };
            this.BaseAddressWithoutScheme = request.BaseAddressWithoutScheme;
            this.UriWithoutScheme = request.UriWithoutScheme;
            this.PathAndQuery = request.PathAndQuery;
            this.RequestHeadersChanges = new Dictionary<string, string>();
        }

        public Action<string> SetResponseContentType { get; set; }

        internal List<QueryStringParam> QueryParams { get; set; }

        public bool ContinueToOtherOwinPipeline { get; set; }

        internal Tuple<HttpStatusCode, string> BlockRequestWith { get; set; }

        private IOwinContext Context { get; }

        internal IDictionary<string, string> RequestHeadersChanges { get; set; }

        internal string Referer { get; set; }

        public string PathAndQuery { get; internal set; }

        public string BaseAddressWithoutScheme { get; internal set; }

        /// <summary>
        ///     No slash prefix
        /// </summary>
        public string UriWithoutScheme { get; internal set; }

        internal object ReturnObjectAsJson { get; set; }

        internal string Forwarding { get; set; }

        internal string ReturnString { set; get; }

        internal string RedirectToString { set; get; }

        /// <summary>
        ///     Add an extension to an extensionless route to same domain
        /// </summary>
        public Func<string, string> ExtensionlessWithExtension { get; internal set; }

        /// <summary>
        ///     Respond with the specified string
        /// </summary>
        public Func<string, string> RespondWithString { get; internal set; }

        /// <summary>
        ///     rewrite by replacing the domain porting, essentially proxy to another domain
        /// </summary>
        public Func<string, string> ForwardToDomain { get; internal set; }

        /// <summary>
        ///     Deny the current route and return specified HttpStatusCode and message
        /// </summary>
        public Func<HttpStatusCode, string, string> DenyWith { get; internal set; }

        /// <summary>
        ///     This will allow the request to continue to the rest of the owin pipeline
        /// </summary>
        public Func<string> Allow { get; internal set; }

        /// <summary>
        ///     Deny the current route
        /// </summary>
        public Func<string> Deny { get; internal set; }

        /// <summary>
        ///     Respond with the specified object. Object is serialized to json
        /// </summary>
        public Func<object, string> RespondWithObjectAsJson { get; internal set; }

        /// <summary>
        ///     Ignores all other settings and just redirects the request to the specified link
        /// </summary>
        public Func<string, string> RedirectTo { get; internal set; }

        /// <summary>
        ///     Add an extension to an extensionless route to another domain so that www.abc.com/index -> www.xyz.com/index.html
        /// </summary>
        public Func<string, string, string> ExtensionlessWithExtensionAndDomain { get; internal set; }

        public string QueryStringWithPrefix { get; internal set; }

        /// <summary>
        ///     No slash prefix
        /// </summary>
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

        public string QueryStringValueByName(string name)
        {
            return (this.QueryParams.FirstOrDefault(x => this.Request.Settings.CaseSensitive ? x.Name == name : x.Name.ToLower() == name.ToLower()) ?? new QueryStringParam(name, "")).Value;
        }

        public bool QueryStringContainsName(string name)
        {
            return this.QueryParams.Exists(x => this.Request.Settings.CaseSensitive ? x.Name == name : x.Name.ToLower() == name.ToLower());
        }

        public bool HasNoQueryString()
        {
            return this.QueryParams.Count == 0;
        }

        public string UseReferer(string referer, string route)
        {
            this.Referer = referer;
            return route;
        }

        public string UseRequestHeaders(Action<IDictionary<string, string>> headers, string route)
        {
            var hdrs = (IDictionary<string, string>)this.Context.Environment["owin.RequestHeaders"];
            headers(hdrs);
            this.RequestHeadersChanges = hdrs;
            return route;
        }
    }
}