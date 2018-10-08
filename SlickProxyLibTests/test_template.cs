namespace SlickProxyLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    public class test_template
    {
        [TestMethod]
        public async Task match_with_regex()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            int otherPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort, otherPort },
                handle =>
                {
                    handle.When(
                        "proxyToOther/(.*)",
                        "proxyToOther/(.*)",
                        request => $"http://localhost:{otherPort}/api/" + request.Part(1));
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
    }
}