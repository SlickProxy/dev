using Microsoft.Owin;
using SlickProxyLibSample;

[assembly: OwinStartup(typeof(StartUp))]

namespace SlickProxyLibSample
{
    using System;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using SlickProxyLib;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSlickProxy(
                request =>
                {
                    request.When(req => request.Uri.EndsWith("/index"), req => $"{req.BaseAddressWithScheme}/index.html");
                    request.When("/cdn/(.*)", "/cdn/(.*)", req => $"{req.Scheme}://code.jquery.com/{req.Part(1)}");
                    request.OnRewriteStarted((f, t) => Console.WriteLine($"Started from {f} to {t} ..."));
                    request.OnRewriteEnded((f, t) => Console.WriteLine($"Ended from {f} to {t} ..."));
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