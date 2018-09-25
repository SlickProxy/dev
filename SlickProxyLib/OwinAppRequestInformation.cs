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
        public Stream Body;
        public CancellationToken CancellationToken;
        public string Method;
        public string OwinVersion;
        public string Path;
        public string PathBase;
        public string Protocol;
        public string QueryString;
        public Stream RequestBody;
        public IDictionary<string, string[]> RequestHeaders;
        public string Res;
        public IDictionary<string, string[]> ResponseHeaders;
        public string Scheme;
        public string Uri;

        public OwinAppRequestInformation(IDictionary<string, object> env)
        {
            //request.When("/cdn/(.*)", "/cdn/(.*)", (req) => $"{req.Scheme}://{req.BaseAddress}/{req.Path}");
            this.RequestBody = (Stream)env["owin.RequestBody"];
            this.RequestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
            this.Method = (string)env["owin.RequestMethod"];
            this.Path = (string)env["owin.RequestPath"];
            this.PathBase = (string)env["owin.RequestPathBase"];
            this.Protocol = (string)env["owin.RequestProtocol"];
            this.QueryString = (string)env["owin.RequestQueryString"];
            this.Scheme = (string)env["owin.RequestScheme"];
            this.Body = (Stream)env["owin.ResponseBody"];
            this.ResponseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            this.OwinVersion = (string)env["owin.Version"];
            this.CancellationToken = (CancellationToken)env["owin.CallCancelled"];
            this.BaseAddress = (string)env["owin.RequestScheme"] + "://" + this.RequestHeaders["Host"].First() +
                (string)env["owin.RequestPathBase"];
            this.Uri = this.BaseAddress + (string)env["owin.RequestPath"];
            this.RemoteIpAddress = (string)env["server.RemoteIpAddress"];
            if (env["owin.RequestQueryString"] != "")
                this.Uri += "?" + (string)env["owin.RequestQueryString"];

            this.Res = $"{this.Method} {this.Uri}";

            this.ProxyObject = new ProxyObject
            {
                BaseAddress = this.BaseAddress,
                Path = this.Path,
                Scheme = this.Scheme,
                Part = i => "",
                Method = this.Method,
                Protocol = this.Protocol,
                QueryString = this.QueryString,
                Port = this.Port
            };
        }

        public string Port { get; set; }

        public string RemoteIpAddress { get; set; }

        public string BaseAddress { get; set; }

        internal string RewriteToUrl { get; set; }

        internal bool IsMatched { set; get; }

        ProxyObject ProxyObject { get; }

        internal Action<string, string> OnRewritingStarted { set; get; }

        internal Action<string, string> OnRewritingEnded { set; get; }

        internal Action<string, OwinAppRequestInformation, Exception> OnRewritingException { set; get; }

        public void When(Func<ProxyObject, bool> test, Func<ProxyObject, string> apply)
        {
            if (this.IsMatched)
                return;

            if (test(this.ProxyObject))
            {
                this.IsMatched = true;
                string to = apply(
                    new ProxyObject
                    {
                        BaseAddress = this.BaseAddress,
                        Path = this.Path,
                        Scheme = this.Scheme,
                        Part = i => "",
                        Method = this.Method,
                        Protocol = this.Protocol,
                        QueryString = this.QueryString,
                        Port = this.Port
                    });
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

        public void When(string m, string m2, Func<ProxyObject, string> apply)
        {
            if (this.IsMatched)
                return;
            Match match = Regex.Match(this.Uri, m);
            Match rep = Regex.Match(this.Uri, m2);
            if (match.Success)
            {
                this.IsMatched = true;
                string to = apply(
                    new ProxyObject
                    {
                        BaseAddress = this.BaseAddress,
                        Path = this.Path,
                        Scheme = this.Scheme,
                        Part = i => rep.Groups[i].Value,
                        Method = this.Method,
                        Protocol = this.Protocol,
                        QueryString = this.QueryString,
                        Port = this.Port
                    });
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