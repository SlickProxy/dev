namespace SlickProxyLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    public class multiple_proxy_tests
    {
        [TestMethod]
        public async Task all_proxyservers_should_run()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            int otherPort1 = TestHelper.FreeTcpPort();
            int otherPort2 = TestHelper.FreeTcpPort();
            int otherPort3 = TestHelper.FreeTcpPort();
            int otherPort4 = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort, otherPort1, otherPort2, otherPort3, otherPort4 },
                handle =>
                {
                    handle.When(
                        "proxyToOther1/(.*)",
                        "proxyToOther1/(.*)",
                        request => $"http://localhost:{otherPort1}/api/" + request.Part(1));

                    handle.When(
                        "proxyToOther2/(.*)",
                        "proxyToOther2/(.*)",
                        request => $"http://localhost:{otherPort2}/api/" + request.Part(1));

                    handle.When(
                        "proxyToOther3/(.*)",
                        "proxyToOther3/(.*)",
                        request => $"http://localhost:{otherPort3}/api/" + request.Part(1));

                    handle.When(
                        "proxyToOther4/(.*)",
                        "proxyToOther4/(.*)",
                        request => $"http://localhost:{otherPort4}/api/" + request.Part(1));
                },
                (client, servers) =>
                {
                    string resultOther1 = client.GetStringAsync($"http://localhost:{otherPort1}/api/values/GetAll").Result;
                    string resultOther2 = client.GetStringAsync($"http://localhost:{otherPort2}/api/values/GetAll").Result;
                    string resultOther3 = client.GetStringAsync($"http://localhost:{otherPort3}/api/values/GetAll").Result;
                    string resultOther4 = client.GetStringAsync($"http://localhost:{otherPort4}/api/values/GetAll").Result;

                    string resultProxy = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;

                    string resultProxyToOther1 = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther1/values/GetAll").Result;
                    string resultProxyToOther2 = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther2/values/GetAll").Result;
                    string resultProxyToOther3 = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther3/values/GetAll").Result;
                    string resultProxyToOther4 = client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOther4/values/GetAll").Result;

                    Assert.ThrowsException<AggregateException>(() => client.GetStringAsync($"http://localhost:{proxyPort}/proxyToOtherOne/values/GetAll").Result);

                    Assert.AreNotEqual(resultProxy, resultProxyToOther1);
                    Assert.AreNotEqual(resultProxy, resultProxyToOther2);
                    Assert.AreNotEqual(resultProxy, resultProxyToOther3);
                    Assert.AreNotEqual(resultProxy, resultProxyToOther4);

                    Assert.AreEqual(resultOther1, resultProxyToOther1);
                    Assert.AreEqual(resultOther2, resultProxyToOther2);
                    Assert.AreEqual(resultOther3, resultProxyToOther3);
                    Assert.AreEqual(resultOther4, resultProxyToOther4);
                });
        }
    }
}