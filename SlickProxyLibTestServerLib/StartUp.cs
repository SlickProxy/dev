using Microsoft.Owin;
using SlickProxyLibTestServerLib;

[assembly: OwinStartup(typeof(StartUp))]

namespace SlickProxyLibTestServerLib
{
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            TestServer.AppBuilder?.Invoke(app);

            if (TestServer.FolderNameToServe != null)
            {
                string uiFolder = TestServer.FolderNameToServe; // AppDomain.CurrentDomain.BaseDirectory + "/../../public";
                var fileSystem = new PhysicalFileSystem(uiFolder);
                var options = new FileServerOptions
                {
                    EnableDirectoryBrowsing = true,
                    FileSystem = fileSystem,
                    EnableDefaultFiles = true
                };
                app.UseFileServer(options);
            }

            SetUpWebApi(app);
        }

        private static void SetUpWebApi(IAppBuilder app)
        {
            //app.Map(
            //    map =>
            //    {
            //        map.UseCors(CorsOptions.AllowAll);
            //    });

            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new ControllerResolver());
            config.MapHttpAttributeRoutes();
            config.Routes.IgnoreRoute("elmah", "{resource}.axd/{*pathInfo}");
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute("FilesRoute", "{*pathInfo}", null, null, new StopRoutingHandler());

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            JsonMediaTypeFormatter jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.UseDataContractJsonSerializer = false; // defaults to false, but no harm done
            jsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            jsonFormatter.SerializerSettings.Formatting = Formatting.None;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            app.UseWebApi(config);
        }
    }
}