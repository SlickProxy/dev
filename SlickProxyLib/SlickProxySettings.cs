namespace SlickProxyLib
{
    using System;
    using System.Net.Http;
    using System.Text.RegularExpressions;

    public class SlickProxySettings
    {
        internal Action RequireAuthenticationWhenRouting;
        internal Action RequireAuthenticationWhenRouting2;

        internal OwinAppRequestInformation @this { get; set; }

        public bool RouteSameServerRewritesOverNetwork { get; set; }

        public bool CaseSensitive { set; get; }

        internal Action<string, string,string, bool> OnRewritingStarted { set; get; }

        internal Action<string, string,string> OnProcessingEnded { set; get; }
        internal Action<ResponseInspection> CollectRequestResponse { set; get; }

        internal Action<string, string,string> OnRewriteToCurrentServer { set; get; }

        internal Action<string, string,string> OnRedirectTo { set; get; }

        internal Action<string, string,string, HttpRequestMessage, Exception> OnRewritingException { set; get; }

        internal Action<string, string,string> OnRewriteToDifferentServer { get; set; }

        internal Action<string> OnNoMatching { get; set; }

        internal Action<string, string,string, HttpRequestMessage, HttpResponseMessage, Exception, string> OnRespondingFromRemoteServer { get; set; }

        internal Action<string> OnRouteBlocking { get; set; }

        public void RequireAuthenticationWhen(string regexMatch)
        {
            this.RequireAuthenticationWhenRouting = () =>
            {
                Match match = this.CaseSensitive ? Regex.Match(this.@this.Uri, regexMatch) : Regex.Match(this.@this.Uri, regexMatch, RegexOptions.IgnoreCase);
                if (match.Success)
                    this.@this.RequireAuthentication = true;
            };
        }

        public void RequireAuthenticationWhen(Func<ProxyObjectWithPath, bool> test)
        {
            this.RequireAuthenticationWhenRouting2 = () =>
            {
                if (test(this.@this.ProxyObject))
                    this.@this.RequireAuthentication = true;
            };
        }

        public void OnAllowed(Action<string> onAction)
        {
            this.@this.OnAllowedToContinue = onAction;
        }

        public void OnRewriteStarted(Action<string, string,string, bool> onAction)
        {
            this.OnRewritingStarted = onAction;
        }

        public void OnRewriteEnded(Action<string, string,string> onAction)
        {
            this.OnProcessingEnded = onAction;
        }

        public void OnInspectRequestResponse(Action<ResponseInspection> onAction)
        {
            this.CollectRequestResponse = onAction;
        }

        public void OnNoMatch(Action<string> onAction)
        {
            this.OnNoMatching = onAction;
        }

        public void OnResponseFromRemoteServer(Action<string, string,string, HttpRequestMessage, HttpResponseMessage, Exception, string> onAction)
        {
            this.OnRespondingFromRemoteServer = onAction;
        }

        public void OnRewriteToCurrentHost(Action<string, string,string> onAction)
        {
            this.OnRewriteToCurrentServer = onAction;
        }

        public void OnRewriteToDifferentHost(Action<string, string,string> onAction)
        {
            this.OnRewriteToDifferentServer = onAction;
        }

        public void OnRedirect(Action<string, string,string> onAction)
        {
            this.OnRedirectTo = onAction;
        }

        public void OnRewriteException(Action<string, string,string, HttpRequestMessage, Exception> onAction)
        {
            this.OnRewritingException = onAction;
        }
    }
}