namespace SlickProxyLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;
    using SlickProxyLibTestServerLib;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class SampleTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            using (TestServer.Instance.Run(settings => settings.AddDefinition(HttpMethod.Get, "server1"), 9909))
            {
                using (TestServer.Instance.Run(settings => settings.AddDefinition(HttpMethod.Get, "server2"), 9908))
                {
                    using (var client = new HttpClient())
                    {
                        string s1 = await client.GetStringAsync("http://localhost:9909/api/values/GetAll");
                        string s2 = await client.GetStringAsync("http://localhost:9908/api/values/GetAll");
                    }
                }
            }
        }

        [TestMethod]
        public async Task TestMethod()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            int otherPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort, otherPort },
                handle =>
                {
                    handle.When(
                        "api/(.*)",
                        request => $"http://localhost:{otherPort}" + request.PathAndQuery);
                },
                (client, servers) =>
                {
                    string result1 = client.GetStringAsync($"http://localhost:{otherPort}/api/values/GetAll").Result;
                    string result2 = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                    Assert.AreEqual(result1, result2);
                });
        }
    }
}