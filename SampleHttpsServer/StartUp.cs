using Microsoft.Owin;
using SampleHttpsServer;

[assembly: OwinStartup(typeof(StartUp))]

namespace SampleHttpsServer
{
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using SlickProxyLib;
    using System;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;

    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSlickProxy(handle => { });

            string uiFolder = AppDomain.CurrentDomain.BaseDirectory + "/../../public";
            var fileSystem = new PhysicalFileSystem(uiFolder);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };
            app.UseFileServer(options);

            SetUpWebApi(app);
        }

        private static void SetUpWebApi(IAppBuilder app)
        {
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