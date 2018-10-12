namespace SlickProxyLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;

    public class SlickProxySettings
    {
        internal Action RequireAuthenticationWhenRouting;
        internal Action RequireAuthenticationWhenRouting2;

        public SecurityProtocolType? SecurityProtocolType { set; get; }

        internal OwinAppRequestInformation @this { get; set; }

        public bool RouteSameServerRewritesOverNetwork { get; set; }

        public bool CaseSensitive { set; get; }

        internal Action<string, string, string, bool> OnRewritingStarted { set; get; }

        internal Action<string, string, string> OnProcessingEnded { set; get; }

        internal Action<ResponseInspection> CollectRequestResponse { set; get; }

        internal Action<string, string, string> OnRewriteToCurrentServer { set; get; }

        internal Action<string, string, string> OnRedirectTo { set; get; }

        internal Action<string, string, string, HttpRequestMessage, Exception> OnRewritingException { set; get; }

        internal Action<string, string, string> OnRewriteToDifferentServer { get; set; }

        internal Action<string> OnNoMatching { get; set; }

        internal Action<string, string, string, HttpRequestMessage, HttpResponseMessage, Exception, string> OnRespondingFromRemoteServer { get; set; }

        internal Action<string> OnRouteBlocking { get; set; }

        internal Action<string, string, string, HttpResponseMessage> BeforeResponding { get; set; }

        public SlickProxySettings OnBeforeResponding(Action<string, string, string, HttpResponseMessage> action)
        {
            this.BeforeResponding = action;
            return this;
        }

        public SlickProxySettings RequireAuthenticationWhen(string regexMatch)
        {
            this.RequireAuthenticationWhenRouting = () =>
            {
                Match match = this.CaseSensitive ? Regex.Match(this.@this.Uri, regexMatch) : Regex.Match(this.@this.Uri, regexMatch, RegexOptions.IgnoreCase);
                if (match.Success)
                    this.@this.RequireAuthentication = true;
            };
            return this;
        }

        public SlickProxySettings RequireAuthenticationWhen(Func<ProxyObjectWithPath, bool> test)
        {
            this.RequireAuthenticationWhenRouting2 = () =>
            {
                if (test(this.@this.ProxyObject))
                    this.@this.RequireAuthentication = true;
            };
            return this;
        }

        public SlickProxySettings OnAllowed(Action<string> onAction)
        {
            this.@this.OnAllowedToContinue = onAction;
            return this;
        }

        public SlickProxySettings OnRewriteStarted(Action<string, string, string, bool> onAction)
        {
            this.OnRewritingStarted = onAction;
            return this;
        }

        public SlickProxySettings OnRewriteEnded(Action<string, string, string> onAction)
        {
            this.OnProcessingEnded = onAction;
            return this;
        }

        /// <summary>
        ///     Setting this will double the request to the remote server
        /// </summary>
        /// <param name="onAction"></param>
        [Obsolete("WARNING : Setting this will double the request to the remote server!")]
        public SlickProxySettings OnInspectRequestResponse(Action<ResponseInspection> onAction)
        {
            this.CollectRequestResponse = onAction;
            return this;
        }

        public SlickProxySettings OnNoMatch(Action<string> onAction)
        {
            this.OnNoMatching = onAction;
            return this;
        }

        public SlickProxySettings OnResponseFromRemoteServer(Action<string, string, string, HttpRequestMessage, HttpResponseMessage, Exception, string> onAction)
        {
            this.OnRespondingFromRemoteServer = onAction;
            return this;
        }

        public SlickProxySettings OnRewriteToCurrentHost(Action<string, string, string> onAction)
        {
            this.OnRewriteToCurrentServer = onAction;
            return this;
        }

        public SlickProxySettings OnRewriteToDifferentHost(Action<string, string, string> onAction)
        {
            this.OnRewriteToDifferentServer = onAction;
            return this;
        }

        public SlickProxySettings OnRedirect(Action<string, string, string> onAction)
        {
            this.OnRedirectTo = onAction;
            return this;
        }

        public SlickProxySettings OnRewriteException(Action<string, string, string, HttpRequestMessage, Exception> onAction)
        {
            this.OnRewritingException = onAction;
            return this;
        }

        public SlickProxySettings LoadBalancer(bool enabled,params LoadBalance[] routeTable)
        {
            this.LoadBalanceList = routeTable.ToList();
            LoadBalanceEnabled = enabled;
            return this;
        }
        internal List<LoadBalance> LoadBalanceList { set; get; }

        internal bool  LoadBalanceEnabled { set; get; }
    }

    public class LoadBalance
    {
        public LoadBalance(params string[] mapping)
        {
            LoadBalanceStrategy = LoadBalanceStrategy.RoundRobin;
           this.Mapping = mapping?.Select(x => new RouteMap(x))?.ToList() ?? new List<RouteMap>();
        }
       
       
        internal List<RouteMap> Mapping { private set; get; }
        internal LoadBalanceStrategy LoadBalanceStrategy { set; get; }

    }
    public enum LoadBalanceStrategy
    {
        RoundRobin
    }
    
    internal class RouteMap
        {
            ulong accessCount;

            public RouteMap(string link)
            {
                this.Host = link;
            }

            public string Host { private set; get; }

            public ulong AccessCount
            {
                set => this.accessCount = value >= 1000000 ? (ulong)0 : value;
                get => this.accessCount;
            }
        }
}