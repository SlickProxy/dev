using Microsoft.Owin;
using SampleProxyServer;

[assembly: OwinStartup(typeof(StartUp))]

namespace SampleProxyServer
{
    using Owin;
    using SlickProxyLib;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = new SlickProxySettings();

            app.UseSlickProxy(handle => handle.RemoteProxyWhenAny("https://forums.asp.net"), settings);

            settings.OnRewriteStarted((from, to,method, sameServer) => Console.WriteLine($"Started {method} from {from} to {to} ..."));
            settings.OnRewriteEnded((from, method, to) =>  Console.WriteLine($"Ended  {method}  from {from} to {to} ..."));
             settings.OnInspectRequestResponse(
                 i =>
                 {
                     Console.WriteLine(i.HttpContent.ReadAsStringAsync().Result);
                     //i.SaveResponseToFile();
                 });
        }
    }
}