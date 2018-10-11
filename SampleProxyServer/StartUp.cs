using Microsoft.Owin;
using SampleProxyServer;

[assembly: OwinStartup(typeof(StartUp))]

namespace SampleProxyServer
{
    using Owin;
    using SlickProxyLib;
    using System;
    using System.Net;
    using System.Net.Http;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = new SlickProxySettings();

            app.UseSlickProxy(handle => handle.RemoteProxyWhenAny("https://forums.asp.net"), settings);
            settings.OnRewriteStarted((from, to, method, sameServer) => Console.WriteLine($"Started {method} from {from} to {to} ..."));
            settings.OnRewriteEnded((from, method, to) => Console.WriteLine($"Ended  {method}  from {from} to {to} ..."));
            settings.SecurityProtocolType = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
            settings.OnBeforeResponding(
                (from, to, method, responseMessage) =>
                {
                    // responseMessage.Content.Headers.ContentType.MediaType == "text/html"
                    if (to.EndsWith(".html"))
                    {
                        string text = responseMessage.Content.ReadAsStringAsync().Result;
                        text.Replace("https://", "");
                        responseMessage.Content = new StringContent(text);
                    }
                });
            settings.OnInspectRequestResponse(
                i =>
                {
                    i.SaveResponsesToFolder(
                        "Z://DownloadSite",
                        "index.html",
                        new Tuple<string, string>("text/html", ".html"),
                        new Tuple<string, string>("application/json", ".json"));
                });
        }
    }
}