namespace SlickProxyLibTests
{
    using SlickProxyLib;
    using SlickProxyLibTestServerLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;

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

        public static async Task Run(List<int> serverPorts, Action<OwinAppRequestInformation> proxyHandler, Action<HttpClient, Dictionary<int, ServerSetUp>> checks)
        {
            using (TestServer.Instance)
            {
                var setups = new List<IDisposable>();
                try
                {
                    int count = 0;
                    foreach (int serverPort in serverPorts)
                    {
                        count++;
                        if (count == 1)
                            setups.Add(
                                TestServer.Instance.Run(
                                    settings =>
                                    {
                                        settings.AddDefinition(HttpMethod.Get, serverPort);
                                        settings.AppBuilder = app => app.UseSlickProxy(proxyHandler);
                                    },
                                    serverPort));
                        else
                            setups.Add(TestServer.Instance.Run(settings => settings.AddDefinition(HttpMethod.Get, serverPort), serverPort));
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