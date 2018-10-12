namespace SlickProxyLibTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;

    [TestClass]
    public class when_routing_from_proxy_to_a_second_server
    {
        public static void TestRouteFromProxyServerToAnother(Action<int, int, OwinAppRequestInformation> setup)
        {
            bool result = Task.Run(
                async () =>
                {
                    int proxyPort = TestHelper.FreeTcpPort();
                    int otherPort = TestHelper.FreeTcpPort();
                    await TestHelper.Run(
                        new List<int>
                            { proxyPort, otherPort },
                        new SlickProxySettings[] { },
                        new Action<OwinAppRequestInformation>[] { handle => { setup(proxyPort, otherPort, handle); } },
                        (client, servers) =>
                        {
                            string resultOther = client.GetStringAsync($"http://localhost:{otherPort}/api/values/GetAll").Result;
                            string resultProxy = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                            string resultProxyToOther = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther/values/GetAll").Result;

                            Assert.ThrowsException<AggregateException>(() => client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOtherOne/values/GetAll").Result);
                            Assert.AreNotEqual(resultProxy, resultProxyToOther);
                            Assert.AreNotEqual(resultProxy, resultOther);
                            Assert.AreEqual(resultOther, resultProxyToOther);
                        });
                    return await Task.FromResult(true);
                }).Result;
        }

        [TestMethod]
        public async Task match_with_regex()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            int otherPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort, otherPort },
                new SlickProxySettings[] { },
                new Action<OwinAppRequestInformation>[]
                {
                    handle =>
                    {
                        handle.When(
                            "proxyToOther/(.*)",
                            "proxyToOther/(.*)",
                            request => $"http://localhost:{otherPort}/api/" + request.Part(1));
                    }
                },
                (client, servers) =>
                {
                    string resultOther = client.GetStringAsync($"http://localhost:{otherPort}/api/values/GetAll").Result;
                    string resultProxy = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                    string resultProxyToOther = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther/values/GetAll").Result;

                    Assert.ThrowsException<AggregateException>(() => client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOtherOne/values/GetAll").Result);
                    Assert.AreNotEqual(resultProxy, resultProxyToOther);
                    Assert.AreNotEqual(resultProxy, resultOther);
                    Assert.AreEqual(resultOther, resultProxyToOther);
                });
        }

        [TestMethod]
        public void match_with_regex2()
        {
            TestRouteFromProxyServerToAnother(
                (proxyPort, otherPort, handle) =>
                {
                    handle.When(
                        "proxyToOther/(.*)",
                        "proxyToOther/(.*)",
                        request => $"http://localhost:{otherPort}/api/" + request.Part(1));
                });
        }

        [TestMethod]
        public void match_with_regex3()
        {
            TestRouteFromProxyServerToAnother(
                (proxyPort, otherPort, handle) =>
                {
                    handle.When(
                        "proxyToOther/(.*)",
                        request => $"http://localhost:{otherPort}/api/values/GetAll");
                });
        }

        [TestMethod]
        public void match_with_PathAndQuery()
        {
            TestRouteFromProxyServerToAnother(
                (proxyPort, otherPort, handle) =>
                {
                    handle.When(
                        request => request.PathAndQuery.StartsWith("/proxyToOther/"),
                        request => $"http://localhost:{otherPort}/api/values/GetAll");
                });
        }

        [TestMethod]
        public void match_with_regex4()
        {
            TestRouteFromProxyServerToAnother(
                (proxyPort, otherPort, handle) =>
                {
                    handle.When(
                        "proxyToOther/(.*)",
                        "proxyToOther/(.*)/(.*)",
                        request => $"http://localhost:{otherPort}/api/{request.Part(1)}/{request.Part(2)}");
                });
        }

        [TestMethod]
        public void match_with_regex5()
        {
            Assert.ThrowsException<AggregateException>(
                () => TestRouteFromProxyServerToAnother(
                    (proxyPort, otherPort, handle) =>
                    {
                        handle.When(
                            "proxyToOther/(.*)",
                            "proxyToOther/(.*)/(.*)",
                            request => $"http://localhost:{otherPort}/api/{request.Part(0)}/{request.Part(1)}");
                    }));
        }

        [TestMethod]
        public void match_with_regex6()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    TestRouteFromProxyServerToAnother(
                        (proxyPort, otherPort, handle) =>
                        {
                            handle.When(
                                "proxyToOther/(.*)",
                                request => $"http://localhost:{otherPort}/api/values/GetAll");
                        });
                    throw new InvalidOperationException();
                });
        }

        [TestMethod]
        public void match_with_regex7()
        {
            Assert.ThrowsException<AggregateException>(
                () =>
                {
                    TestRouteFromProxyServerToAnother(
                        (proxyPort, otherPort, handle) =>
                        {
                            handle.When(
                                "proxyToOther/(.*)",
                                "proxyToOther/(.*)/(.*)",
                                request => $"http://localhost:{otherPort}/api/{request.Part(0)}/{request.Part(1)}");
                        });
                    throw new InvalidOperationException();
                });
        }

        [TestMethod]
        public void match_with_regex8()
        {
            Assert.ThrowsException<AggregateException>(
                () => TestRouteFromProxyServerToAnother(
                    (proxyPort, otherPort, handle) =>
                    {
                        handle.When(
                            "proxyToOther/(.*)",
                            "proxyToOther/(.*)/(.*)",
                            request => $"http://localhost:{otherPort}/api/{request.Part(2)}/{request.Part(3)}");
                    }));
        }
    }
}