namespace SlickProxyLibTestServerLib
{
    using Microsoft.Owin.Hosting;
    using Newtonsoft.Json;
    using Owin;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;

    public class TestServer : IDisposable
    {
        private static readonly Lazy<TestServer> Lazy = new Lazy<TestServer>(() => new TestServer());

        private TestServer()
        {
            this.Servers = new Dictionary<int, ServerSetUp>();
        }

        public static TestServer Instance => Lazy.Value;

        internal static Action<IAppBuilder> AppBuilder { set; get; }

        internal static string FolderNameToServe { set; get; }

        public Dictionary<int, ServerSetUp> Servers { internal set; get; }

        public IDisposable Webapp { get; set; }

        public void Dispose()
        {
            this.Webapp?.Dispose();

            foreach (KeyValuePair<int, ServerSetUp> serverSetUp in this.Servers)
            {
                if (ValuesController.RequestResponseDefinition.ContainsKey(serverSetUp.Key))
                    ValuesController.RequestResponseDefinition[serverSetUp.Key] = new Dictionary<HttpMethod, Dictionary<string, RequestConstruct>>();
                serverSetUp.Value.PortNumber = 0;
            }
        }

        internal static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public IDisposable Run(Action<ServerSetUp> setUpFun = null, int? port = null, string scheme = null)
        {
            var serverSetUp = new ServerSetUp(port)
            {
                ServerDefinitions = new List<ServerDefinition>()
            };
            if (setUpFun == null)
                setUpFun = s => { };

            setUpFun(serverSetUp);
            if (serverSetUp.PortNumber == 0 || this.Servers.ContainsKey(serverSetUp.PortNumber))
                throw new Exception($"Server with port {serverSetUp.PortNumber} is not allowed or has already been created");

            AppBuilder = serverSetUp.AppBuilder;
            FolderNameToServe = serverSetUp.FolderNameToServe;
            string httpLocalhost = serverSetUp.Scheme + "://localhost:" + serverSetUp.PortNumber.ToString();

            ValuesController.RequestResponseDefinition.Add(
                serverSetUp.PortNumber,
                new Dictionary<HttpMethod, Dictionary<string, RequestConstruct>>
                {
                    { HttpMethod.Get, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Post, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Put, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Delete, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Options, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Head, new Dictionary<string, RequestConstruct>() },
                    { HttpMethod.Trace, new Dictionary<string, RequestConstruct>() }
                });

            foreach (ServerDefinition serverDefinition in serverSetUp.ServerDefinitions)
                ValuesController.RequestResponseDefinition[serverSetUp.PortNumber][serverDefinition.Method].Add(
                    serverDefinition.Argument == null ? "" : JsonConvert.SerializeObject(serverDefinition.Argument),
                    new RequestConstruct
                    {
                        Response = serverDefinition.Response,
                        ResponseStatusCode = serverDefinition.ResponseStatusCode
                    });
            this.Servers.Add(serverSetUp.PortNumber, serverSetUp);
            this.Webapp = WebApp.Start<StartUp>(httpLocalhost);
            return this.Webapp;
        }
    }
}