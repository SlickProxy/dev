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

            app.UseSlickProxy(handle => handle.RemoteProxyWhenAny("http://lettuce.ancorathemes.com"), settings);
            settings.LoadBalancer(enabled: true,routeTable: new[]
            {
                new LoadBalance("google.com","stackoverflow.com","abc.com")
            });

            settings.OnRewriteStarted((from, to, method, sameServer) => Console.WriteLine($"Started {method} from {from} to {to} ..."));
            settings.OnRewriteEnded((from, method, to) => Console.WriteLine($"Ended  {method}  from {from} to {to} ..."));
            settings.SecurityProtocolType = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
            settings.OnBeforeResponding(
                (from, to, method, responseMessage) =>
                {
                    var mediaType = responseMessage.Content.Headers.ContentType.MediaType;
                    var isHtml = mediaType == "text/html";
                    if (isHtml)
                    {
                        string text = responseMessage.Content.ReadAsStringAsync().Result;
                        text=text.Replace("lettuce.ancorathemes.com", "localhost:9900");
                        responseMessage.Content = new StringContent(text);
                        responseMessage.Content.Headers.ContentType.MediaType= mediaType;
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