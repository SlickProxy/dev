using Microsoft.Owin;
using SlickProxyLibSample;

[assembly: OwinStartup(typeof(StartUp))]

namespace SlickProxyLibSample
{
    using System;
    using System.Diagnostics;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using SlickProxyLib;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHooks(after: env => Console.WriteLine("Completed request to {0}", env["owin.RequestPath"]));
            app.UseHooks(
                env => Stopwatch.StartNew(),
                (stopwatch, env) =>
                {
                    stopwatch.Stop();
                    Console.WriteLine(
                        "Request completed in {0} milliseconds.",
                        stopwatch.ElapsedMilliseconds);
                }
            );
            //app.UseHooksAsync(before: env => Task.Delay(2000));

            app.UseSlickProxy(
                request =>
                {
                    request.When(req => req.Path.EndsWith("/index"), req => req.ExtensionlessWithExtension("html"));
                    request.When(req => req.Path.EndsWith("/indexString"), req => req.RespondWithString("what's up men!"));
                    request.When(req => req.Path.EndsWith("/indexObject"), req => req.RespondWithObjectAsJson(DateTime.UtcNow));

                    request.When("/cdn2/(.*)", req => req.ForwardToDomain("https://code.jquery.com"));

                    request.When("/cdn/(.*)", "/cdn/(.*)", req => $"{req.Scheme}://code.jquery.com/{req.Part(1)}");

                    //request.When(req => req.Scheme == "http", req => req.RedirectTo(req.AsHttps));

                    request.OnRewriteStarted((f, t) => Console.WriteLine($"Started from {f} to {t} ..."));
                    request.OnRedirect((f, t) => Console.WriteLine($"Redirecting from {f} to {t} ..."));
                    request.OnRewriteEnded((f, t) => Console.WriteLine($"Ended from {f} to {t} ..."));
                    request.OnRewriteToCurrentHost((f, t) => Console.WriteLine($"Rewritten to current server from {f} to {t} ..."));
                    request.OnRewriteException((f, r, e) => Console.WriteLine($"Error when rewriting from {f} gave error ..." + e));
                });

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