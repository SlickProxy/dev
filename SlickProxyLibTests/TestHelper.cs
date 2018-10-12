namespace SlickProxyLibTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using SlickProxyLib;
    using SlickProxyLibTestServerLib;
    public class TestService
    {
        public int? PortNumber { set; get; }
        public SlickProxySettings Settings { set; get; }
        public Action<OwinAppRequestInformation> handler { set; get; }
    }
    public class TestHelper
    {
        internal static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }



        public static async Task Run(Action<TestService>[] services,
            Action<HttpClient, Dictionary<int, ServerSetUp>> checks)
        {
            var serverPorts=new List<int>();
            var appSettings=new List<SlickProxySettings>();
            var proxyHandler=new List<Action<OwinAppRequestInformation>>();

            foreach (Action<TestService> service in services)
            {
                var arg = new TestService()
                {
                    Settings = new SlickProxySettings()
                };
                service?.Invoke(arg);
                if (arg.PortNumber == null)
                {
                    arg.PortNumber = TestHelper.FreeTcpPort();
                }
                serverPorts.Add(arg.PortNumber.Value);
                appSettings.Add(arg.Settings);
                proxyHandler.Add(arg.handler);
            }
             await Run(serverPorts, appSettings.ToArray(), proxyHandler.ToArray(), checks);
        }


        public static async Task Run(
            List<int> serverPorts,
            SlickProxySettings[] appSettings,
            Action<OwinAppRequestInformation>[] proxyHandler,
            Action<HttpClient, Dictionary<int, ServerSetUp>> checks)
        {
            using (TestServer.Instance)
            {
                var setups = new List<IDisposable>();
                try
                {
                    for (int i = 0; i < serverPorts.Count; i++)
                    {
                        int serverPort = serverPorts[i];
                        setups.Add(
                            TestServer.Instance.Run(
                                settings =>
                                {
                                    settings.AddDefinition(HttpMethod.Get, serverPort);
                                    if (i < proxyHandler.Length)
                                    {
                                        if (i < appSettings.Length)
                                            settings.AppBuilder = app => app.UseSlickProxy(proxyHandler[i], appSettings[i]);
                                        else
                                            settings.AppBuilder = app => app.UseSlickProxy(proxyHandler[i]);
                                    }
                                },
                                serverPort));
                    }
                    var servers = new Dictionary<int, ServerSetUp>();
                    foreach (KeyValuePair<int, ServerSetUp> keyValuePair in TestServer.Instance.Servers.Where(x => serverPorts.Exists(y => x.Key == y)))
                        servers.Add(keyValuePair.Key, keyValuePair.Value);

                    using (var client = new HttpClient())
                    {
                        checks?.Invoke(client, servers);
                    }
                }
                finally
                {
                    setups.ForEach(x => x.Dispose());
                }
            }
        }
    }
}