namespace SlickProxyLib
{
    using Microsoft.Owin;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class OwinAppRequestInformation
    {
        internal readonly IOwinContext Context;
        internal CancellationToken CancellationToken;

        internal OwinAppRequestInformation(IOwinContext context, SlickProxySettings settings)
        {
            if (context == null)
                return;
            this.CopySettings(settings);

            this.Context = context;
            this.OwinRequestDictionary = context.Environment;
            this.RequestBody = (Stream)this.OwinRequestDictionary["owin.RequestBody"];
            this.RequestHeaders = (IDictionary<string, string[]>)this.OwinRequestDictionary["owin.RequestHeaders"];
            this.Method = (string)this.OwinRequestDictionary["owin.RequestMethod"];
            this.Path = (string)this.OwinRequestDictionary["owin.RequestPath"];
            this.PathBase = (string)this.OwinRequestDictionary["owin.RequestPathBase"];
            this.Protocol = (string)this.OwinRequestDictionary["owin.RequestProtocol"];
            this.QueryString = (string)this.OwinRequestDictionary["owin.RequestQueryString"];

            this.QueryStringWithPrefix = string.IsNullOrEmpty(this.QueryString) ? "" : "?" + this.QueryString;

            this.Scheme = (string)this.OwinRequestDictionary["owin.RequestScheme"];
            this.ResponseBody = (Stream)this.OwinRequestDictionary["owin.ResponseBody"];
            this.ResponseHeaders = (IDictionary<string, string[]>)this.OwinRequestDictionary["owin.ResponseHeaders"];
            this.OwinVersion = (string)this.OwinRequestDictionary["owin.Version"];
            this.CancellationToken = (CancellationToken)this.OwinRequestDictionary["owin.CallCancelled"];

            this.HostNameWithPort = this.RequestHeaders["Host"].First();
            string[] parts = this.HostNameWithPort.Split(':');
            this.Port = parts.Length == 1 ? "80" : parts[1];
            this.HostName = parts[0];

            this.BaseAddressWithoutScheme = this.HostNameWithPort + this.PathBase;
            this.BaseAddressWithScheme = this.Scheme + "://" + this.HostNameWithPort + this.PathBase;

            this.UriWithoutScheme = this.BaseAddressWithoutScheme + (string)this.OwinRequestDictionary["owin.RequestPath"];
            this.Uri = this.BaseAddressWithScheme + (string)this.OwinRequestDictionary["owin.RequestPath"];
            this.RemoteIpAddress = (string)this.OwinRequestDictionary["server.RemoteIpAddress"];

            this.PathAndQuery = (string)this.OwinRequestDictionary["owin.RequestPath"];

            if (this.OwinRequestDictionary["owin.RequestQueryString"] != "")
            {
                this.Uri += "?" + (string)this.OwinRequestDictionary["owin.RequestQueryString"];
                this.UriWithoutScheme += "?" + (string)this.OwinRequestDictionary["owin.RequestQueryString"];
                this.PathAndQuery += "?" + (string)this.OwinRequestDictionary["owin.RequestQueryString"];
            }

            this.Res = $"{this.Method} {this.Uri}";

            var queryStringParams = new List<QueryStringParam>();
            foreach (string s in this.QueryString.Split('&'))
            {
                string[] pt = s.Split('=');
                if (pt.Length <= 1)
                    continue;
                string name = pt[0];
                string val = pt[1];
                queryStringParams.Add(new QueryStringParam(name, val));
            }

            this.QueryParams = queryStringParams;

            //last thing to be set
            this.ProxyObject = new ProxyObjectWithPath(this, null, context);
        }

        internal SlickProxySettings Settings { set; get; }

        /// <summary>
        ///     Has slash prefix
        /// </summary>
        internal string PathAndQuery { get; set; }

        internal string BaseAddressWithoutScheme { get; set; }

        internal string UriWithoutScheme { get; set; }

        internal string QueryStringWithPrefix { get; set; }

        internal Stream ResponseBody { get; }

        internal string Method { get; }

        internal string OwinVersion { get; }

        internal string Path { get; }

        internal string PathBase { get; }

        internal string Protocol { get; }

        internal string QueryString { get; }

        internal Stream RequestBody { get; }

        internal IDictionary<string, string[]> RequestHeaders { get; }

        internal string Res { get; }

        internal IDictionary<string, string[]> ResponseHeaders { get; }

        internal string Scheme { get; }

        internal string Uri { get; }

        private IDictionary<string, object> OwinRequestDictionary { get; }

        internal string HostName { get; }

        internal string HostNameWithPort { get; }

        internal string Port { get; }

        internal string RemoteIpAddress { get; }

        internal string BaseAddressWithScheme { get; }

        internal string RewriteToUrl { get; set; }

        internal bool IsMatched { set; get; }

        internal ProxyObjectWithPath ProxyObject { get; set; }

        internal Action<string> OnAllowedToContinue { get; set; }

        internal bool RequireAuthentication { set; get; }

        internal List<QueryStringParam> QueryParams { get; set; }

        internal string ResponseContentHeadersContentType { get; set; }

        private void CopySettings(SlickProxySettings settings)
        {
            this.Settings = new SlickProxySettings
            {
                CaseSensitive = settings.CaseSensitive,
                RequireAuthenticationWhenRouting = settings.RequireAuthenticationWhenRouting,
                RequireAuthenticationWhenRouting2 = settings.RequireAuthenticationWhenRouting2,
                RouteSameServerRewritesOverNetwork = settings.RouteSameServerRewritesOverNetwork,
                OnRewriteToCurrentServer = settings.OnRewriteToCurrentServer,
                OnNoMatching = settings.OnNoMatching,
                OnRewriteToDifferentServer = settings.OnRewriteToDifferentServer,
                OnRedirectTo = settings.OnRedirectTo,
                OnRespondingFromRemoteServer = settings.OnRespondingFromRemoteServer,
                OnRewritingException = settings.OnRewritingException,
                OnProcessingEnded = settings.OnProcessingEnded,
                OnRewritingStarted = settings.OnRewritingStarted,
                OnRouteBlocking = settings.OnRouteBlocking,
                CollectRequestResponse = settings.CollectRequestResponse,
                BeforeResponding = settings.BeforeResponding,

                @this = this
            };
        }
    }
}