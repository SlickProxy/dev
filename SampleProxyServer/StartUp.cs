using Microsoft.Owin;
using SampleProxyServer;

[assembly: OwinStartup(typeof(StartUp))]

namespace SampleProxyServer
{
    using Owin;
    using SlickProxyLib;
    using System;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = new SlickProxySettings();
            settings.OnRewriteStarted((from, to, sameServer) => { Console.WriteLine($"Started from {from} to {to} ..."); });
            settings.OnRewriteEnded((from, to) => { Console.WriteLine($"Ended from {from} to {to} ..."); });

            app.UseSlickProxy(handle => handle.RemoteProxyWhenAny("https://localhost:44306"), settings);
        }
    }
}