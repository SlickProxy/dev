namespace SlickProxyLibTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;

    [TestClass]
    public class when_using_load_balancer
    {
        [TestMethod]
        public async Task it_should_load_balance_traffic_with_round_robin()
        {
            int master = TestHelper.FreeTcpPort();
            int slave1 = TestHelper.FreeTcpPort();
            int slave2 = TestHelper.FreeTcpPort();
            int slave3 = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new Action<TestService>[]
                {
                    slickProxy =>
                    {
                        slickProxy.PortNumber = master;
                        slickProxy.Settings.LoadBalancer(enabled:true,
                            routeTable:new LoadBalance(
                                $"http://localhost:{slave1}",
                                $"http://localhost:{slave2}",
                                $"http://localhost:{slave3}"));
                        slickProxy.handler = handle => handle.WhenAny(request => $"http://localhost:{slave1}");
                    },
                    slickProxy =>
                    {
                        slickProxy.PortNumber = slave1;
                        slickProxy.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    },
                    slickProxy =>
                    {
                        slickProxy.PortNumber = slave2;
                        slickProxy.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    },
                    slickProxy =>
                    {
                        slickProxy.PortNumber = slave3;
                        slickProxy.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    }}, 
                (client, servers) =>
                {
                    var results = new List<string>()
                    {
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                    };
                    Assert.AreEqual(results.Count(x=>x==slave1.ToString()),3);
                    Assert.AreEqual(results.Count(x=>x==slave2.ToString()),3);
                    Assert.AreEqual(results.Count(x=>x==slave3.ToString()),3);
                });
        }

        [TestMethod]
        public async Task it_should_not_load_balance_when_not_load_balancing_is_setup()
        {
            int master = TestHelper.FreeTcpPort();
            int slave1 = TestHelper.FreeTcpPort();
            int slave2 = TestHelper.FreeTcpPort();
            int slave3 = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new Action<TestService>[]
                {
                    service =>
                    {
                        service.PortNumber = master;
                        service.handler = handle => handle.WhenAny(request => $"http://localhost:{slave1}");
                    },
                    service =>
                    {
                        service.PortNumber = slave1;
                        service.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    },service =>
                    {
                        service.PortNumber = slave2;
                        service.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    },service =>
                    {
                        service.PortNumber = slave3;
                        service.handler = handle => handle.WhenAny(request => request.RespondWithString(request.Port));
                    }},
                (client, servers) =>
                {

                    var results = new List<string>()
                    {
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                        client.GetStringAsync($"http://localhost:{master}").Result,
                    };
                    Assert.AreEqual(results.Count(x => x == slave1.ToString()), 9);
                    Assert.AreEqual(results.Count(x => x == slave2.ToString()), 0);
                    Assert.AreEqual(results.Count(x => x == slave3.ToString()), 0);
                });
        }
    }
}