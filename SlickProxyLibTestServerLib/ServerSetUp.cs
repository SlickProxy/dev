namespace SlickProxyLibTestServerLib
{
    using Owin;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    public class ServerSetUp
    {
        public ServerSetUp(int? portNumber = null, Action<IAppBuilder> appBuilder = null, string folderNameToServe = null, string scheme = null)
        {
            this.PortNumber = portNumber ?? TestServer.FreeTcpPort();
            this.AppBuilder = appBuilder;
            this.FolderNameToServe = folderNameToServe;
            this.Scheme = scheme ?? "http";
            this.ServerDefinitions = new List<ServerDefinition>();
        }

        public int PortNumber { internal set; get; }

        public string Scheme { get; }

        public Action<IAppBuilder> AppBuilder { set; get; }

        public string FolderNameToServe { set; get; }

        internal List<ServerDefinition> ServerDefinitions { set; get; }

        public void AddDefinition(HttpMethod method, object response = null, object argument = null, HttpStatusCode responseStatusCode = HttpStatusCode.OK)
        {
            this.ServerDefinitions.Add(new ServerDefinition(method, response, argument, responseStatusCode));
        }
    }
}