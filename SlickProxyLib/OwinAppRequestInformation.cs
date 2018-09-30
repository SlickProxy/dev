namespace SlickProxyLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Microsoft.Owin;

    public class OwinAppRequestInformation
    {
        public CancellationToken CancellationToken;

        public OwinAppRequestInformation(IOwinContext context)
        {
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

            this.ProxyObject = new ProxyObjectWithPath(this, null);
        }

        public string PathAndQuery { get; internal set; }

        public string BaseAddressWithoutScheme { get; internal set; }

        public string UriWithoutScheme { get; internal set; }

        public string QueryStringWithPrefix { get; internal set; }

        public Stream ResponseBody { get; }

        public string Method { get; }

        public string OwinVersion { get; }

        public string Path { get; }

        public string PathBase { get; }

        public string Protocol { get; }

        public string QueryString { get; }

        public Stream RequestBody { get; }

        public IDictionary<string, string[]> RequestHeaders { get; }

        public string Res { get; }

        public IDictionary<string, string[]> ResponseHeaders { get; }

        public string Scheme { get; }

        public string Uri { get; }

        IDictionary<string, object> OwinRequestDictionary { get; }

        public string HostName { get; }

        public string HostNameWithPort { get; }

        public string Port { get; }

        public string RemoteIpAddress { get; }

        public string BaseAddressWithScheme { get; }

        internal string RewriteToUrl { get; set; }

        internal bool IsMatched { private set; get; }

        internal ProxyObjectWithPath ProxyObject { get; set; }

        internal Action<string, string> OnRewritingStarted { private set; get; }

        internal Action<string, string> OnRewritingEnded { private set; get; }

        internal Action<string, string> OnRewriteToCurrentServer { private set; get; }

        internal Action<string, string> OnRedirectTo { private set; get; }

        internal Action<string, OwinAppRequestInformation, Exception> OnRewritingException { private set; get; }

        public void When(Func<ProxyObjectWithPath, bool> test, Func<ProxyObject, string> apply)
        {
            if (this.IsMatched)
                return;

            if (test(this.ProxyObject))
            {
                string to = apply(this.ProxyObject);
                this.RewriteToUrl = to;
                this.IsMatched = true;
            }
        }

        public void WhenAny(Func<ProxyObject, string> apply)
        {
            this.When("(*.)", apply);
        }

        public void When(string m, Func<ProxyObject, string> apply)
        {
            this.When(m, m, apply);
        }

        public void When(string m, Action<ProxyObject> apply)
        {
            this.When(
                m,
                m,
                r =>
                {
                    apply?.Invoke(r);
                    return null;
                });
        }

        public void When(Func<ProxyObjectWithPath, bool> test, Action<ProxyObject> apply)
        {
            if (this.IsMatched)
                return;

            if (test(this.ProxyObject))
            {
                apply(this.ProxyObject);
                this.IsMatched = true;
            }
        }

        public void When(string m, string m2, Func<ProxyObjectWithPath, string> apply)
        {
            if (this.IsMatched)
                return;
            Match match = Regex.Match(this.Uri, m);
            Match rep = Regex.Match(this.Uri, m2);
            if (match.Success)
            {
                this.ProxyObject = new ProxyObjectWithPath(this, i => rep.Groups[i].Value);
                string to = apply(this.ProxyObject);
                this.RewriteToUrl = to;
                this.IsMatched = true;
            }
        }

        public void OnRewriteStarted(Action<string, string> onAction)
        {
            this.OnRewritingStarted = onAction;
        }

        public void OnRewriteEnded(Action<string, string> onAction)
        {
            this.OnRewritingEnded = onAction;
        }

        public void OnRewriteToCurrentHost(Action<string, string> onAction)
        {
            this.OnRewriteToCurrentServer = onAction;
        }

        public void OnRedirect(Action<string, string> onAction)
        {
            this.OnRedirectTo = onAction;
        }

        public void OnRewriteException(Action<string, OwinAppRequestInformation, Exception> onAction)
        {
            this.OnRewritingException = onAction;
        }
    }
}