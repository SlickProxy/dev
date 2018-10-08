using Microsoft.Owin;
using SlickProxyLibSample;

[assembly: OwinStartup(typeof(StartUp))]

namespace SlickProxyLibSample
{
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using SlickProxyLib;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            //  app.UseHooks(after: env => Console.WriteLine("Completed requestuest to {0}", env["owin.requestuestPath"]));
            app.UseHooks(
                env => Stopwatch.StartNew(),
                (stopwatch, env) =>
                {
                    stopwatch.Stop();
                    Console.WriteLine(
                        "requestuest completed in {0} milliseconds.",
                        stopwatch.ElapsedMilliseconds);
                }
            );
            //app.UseHooksAsync(before: env => Task.Delay(2000));

            var settings = new SlickProxySettings
            {
                CaseSensitive = false,
                RouteSameServerRewritesOverNetwork = false
            };

            app.UseSlickProxy(handle =>
                {
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseReferer($"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseRequestHeaders(headers => headers["Referer"] = $"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/index"), request => request.ExtensionlessWithExtension("html"));
                    handle.When(request => request.Path.EndsWith("/indexString"), request => request.RespondWithString("what's up men!"));
                    handle.When(request => request.Path.EndsWith("/indexObject"), request => request.RespondWithObjectAsJson(DateTime.UtcNow));
                    handle.When("/cdn2/(.*)", request => request.ForwardToDomain("https://code.jquery.com"));
                    handle.When("/cdn/(.*)", "/cdn/(.*)", request => $"{request.Scheme}://code.jquery.com/{request.Part(1)}");

                    handle.When(request => request.QueryStringValueByName("link") == "hello", req => req.Deny());
                    handle.When("(.*)", req => req.Allow());
                    //the matches below will never be reached coz of the allow above
                    handle.DenyAny(HttpStatusCode.BadRequest);
                    handle.When("(.*)", req => req.DenyWith(HttpStatusCode.OK, null));

                    handle.When(request => request.QueryStringContainsName("bad"), req => req.Deny());
                    handle.When("/deny(.*)", request => request.HasNoQueryString(), req => req.Deny());

                    var list = new List<string>
                        { "" };
                    handle.When("/scene(.*)", req => list.Contains(req.QueryStringValueByName("link")), req => $"{req.BaseAddressWithScheme}");
                },settings);

            //this will run regardless of any previous match
            settings.RequireAuthenticationWhen("helloSir(.*)");
            //this will run regardless of any previous match
            settings.RequireAuthenticationWhen(req => req.PathAndQuery.Contains("wooo"));
            //requestuest.When(request => request.Scheme == "http", request => request.RedirectTo(request.AsHttps));
            settings.OnRewriteStarted((from, to, sameServer) => { Console.WriteLine($"Started from {from} to {to} ..."); });
            settings.OnRedirect((from, to) => { Console.WriteLine($"Redirecting from {from} to {to} ..."); });
            settings.OnRewriteEnded((from, to) => { Console.WriteLine($"Ended from {from} to {to} ..."); });
            settings.OnRewriteToCurrentHost((from, to) => { Console.WriteLine($"Rewritten to current server from {from} to {to} ..."); });
            settings.OnRewriteException((from, to, requestMessage, error) => { Console.WriteLine($"Error when rewriting from {from} to {to} gave error ..." + error); });
            settings.OnResponseFromRemoteServer((from, to, requestMessage, responseMessage, exception, message) => { Console.WriteLine($"When rewriting from {from} to {to} {message} , gave error ..." + exception + $" content {responseMessage.Content.ReadAsStringAsync().Result}"); });
            settings.OnNoMatch(from => { Console.WriteLine($"No match for  {from} "); });
            settings.OnAllowed(from => { Console.WriteLine($"No match for  {from} "); });

            string uiFolder = AppDomain.CurrentDomain.BaseDirectory + "/../../public";

            var fileSystem = new PhysicalFileSystem(uiFolder);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };
            app.UseFileServer(options);
        }
    }
}