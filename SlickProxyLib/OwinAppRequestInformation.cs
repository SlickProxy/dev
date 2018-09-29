namespace SlickProxyLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class OwinAppRequestInformation
    {
        public CancellationToken CancellationToken;

        public OwinAppRequestInformation(IDictionary<string, object> owinRequestDictionary)
        {
            this.OwinRequestDictionary = owinRequestDictionary;
            this.RequestBody = (Stream)this.OwinRequestDictionary["owin.RequestBody"];
            this.RequestHeaders = (IDictionary<string, string[]>)this.OwinRequestDictionary["owin.RequestHeaders"];
            this.Method = (string)this.OwinRequestDictionary["owin.RequestMethod"];
            this.Path = (string)this.OwinRequestDictionary["owin.RequestPath"];
            this.PathBase = (string)this.OwinRequestDictionary["owin.RequestPathBase"];
            this.Protocol = (string)this.OwinRequestDictionary["owin.RequestProtocol"];
            this.QueryString = (string)this.OwinRequestDictionary["owin.RequestQueryString"];
            this.Scheme = (string)this.OwinRequestDictionary["owin.RequestScheme"];
            this.ResponseBody = (Stream)this.OwinRequestDictionary["owin.ResponseBody"];
            this.ResponseHeaders = (IDictionary<string, string[]>)this.OwinRequestDictionary["owin.ResponseHeaders"];
            this.OwinVersion = (string)this.OwinRequestDictionary["owin.Version"];
            this.CancellationToken = (CancellationToken)this.OwinRequestDictionary["owin.CallCancelled"];

            this.HostNameWithPort = this.RequestHeaders["Host"].First();
            string[] parts = this.HostNameWithPort.Split(':');
            this.Port = parts.Length == 1 ? "80" : parts[1];
            this.HostName = parts[0];

            this.BaseAddressWithScheme = this.Scheme + "://" + this.HostNameWithPort + this.PathBase;
            this.Uri = this.BaseAddressWithScheme + (string)this.OwinRequestDictionary["owin.RequestPath"];
            this.RemoteIpAddress = (string)this.OwinRequestDictionary["server.RemoteIpAddress"];
            if (this.OwinRequestDictionary["owin.RequestQueryString"] != "")
                this.Uri += "?" + (string)this.OwinRequestDictionary["owin.RequestQueryString"];

            this.Res = $"{this.Method} {this.Uri}";

            this.ProxyObject = new ProxyObjectWithPath(this, null);
        }

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

        internal bool IsMatched { set; private get; }

        ProxyObjectWithPath ProxyObject { get; }

        internal Action<string, string> OnRewritingStarted { private set; get; }

        internal Action<string, string> OnRewritingEnded { private set; get; }

        internal Action<string, OwinAppRequestInformation, Exception> OnRewritingException { private set; get; }

        public void When(Func<ProxyObjectWithPath, bool> test, Func<ProxyObject, string> apply)
        {
            if (this.IsMatched)
                return;

            if (test(this.ProxyObject))
            {
                this.IsMatched = true;
                string to = apply(new ProxyObjectWithPath(this, null));
                this.RewriteToUrl = to;
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

        public void When(string m, string m2, Func<ProxyObjectWithPath, string> apply)
        {
            if (this.IsMatched)
                return;
            Match match = Regex.Match(this.Uri, m);
            Match rep = Regex.Match(this.Uri, m2);
            if (match.Success)
            {
                this.IsMatched = true;
                string to = apply(new ProxyObjectWithPath(this, i => rep.Groups[i].Value));
                this.RewriteToUrl = to;
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

        public void OnRewriteException(Action<string, OwinAppRequestInformation, Exception> onAction)
        {
            this.OnRewritingException = onAction;
        }
    }
}