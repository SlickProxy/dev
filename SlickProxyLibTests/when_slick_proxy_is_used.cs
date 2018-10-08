namespace SlickProxyLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlickProxyLib;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    public class when_slick_proxy_is_used
    {
        [TestMethod]
        public async Task it_can_respond_with_predefined_data()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", request => request.RespondWithString("hello")); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                    string result2 = client.GetStringAsync($"http://localhost:{proxyPort}").Result;
                    string result3 = client.GetStringAsync($"http://localhost:{proxyPort}/api").Result;
                    Assert.AreEqual("hello", result);
                    Assert.AreEqual("hello", result2);
                    Assert.AreEqual("hello", result3);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_whenAny_is_used()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.WhenAny(request => request.RespondWithString("hello")); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                    string result2 = client.GetStringAsync($"http://localhost:{proxyPort}").Result;
                    string result3 = client.GetStringAsync($"http://localhost:{proxyPort}/api").Result;
                    Assert.AreEqual("hello", result);
                    Assert.AreEqual("hello", result2);
                    Assert.AreEqual("hello", result3);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_as_no_query_string_is_set()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When(request => request.HasNoQueryString(), request => request.RespondWithString("hello")); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api").Result;
                    Assert.ThrowsException<AggregateException>(() => client.GetStringAsync($"http://localhost:{proxyPort}/api?name=sam").Result);
                    Assert.AreEqual("hello", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_as_no_query_string_contains_name()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When(request => request.QueryStringContainsName("name"), request => request.RespondWithString("hello")); },
                (client, servers) =>
                {
                    Assert.ThrowsException<AggregateException>(() => client.GetStringAsync($"http://localhost:{proxyPort}/api").Result);
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api?name=sam").Result;
                    Assert.AreEqual("hello", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Part(0))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("api/path/to/resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request1()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Part(1))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("path", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request2()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Part(2))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("to", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request3()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Part(3))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request4()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Scheme)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("http", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request5()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.Path)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("/api/path/to/resource", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request6()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.PathAndQuery)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("/api/path/to/resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request7()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.PartWithPattern("api/path/(.*)", 0))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("api/path/to/resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request8()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.PartWithPattern("api/path/(.*)", 1))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("to/resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request9()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.PartWithSourceAndPattern("http://localhost:77/api/path/to2/resource2?name2=sam2&data2=fun2", "api/path/(.*)", 1))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual("to2/resource2?name2=sam2&data2=fun2", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request10()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.AsHttps)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual($"https://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request11()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.BaseAddressWithScheme)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual($"http://localhost:{proxyPort}", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request12()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.BaseAddressWithoutScheme)); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual($"localhost:{proxyPort}", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request13()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.ExtensionlessWithExtension("html"))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual($"http://localhost:{proxyPort}/api/path/to/resource.html?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request14()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.ExtensionlessWithExtension("wooo"))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource?name=sam&data=fun").Result;
                    Assert.AreEqual($"http://localhost:{proxyPort}/api/path/to/resource.wooo?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_respond_with_predefined_data_extracting_parts_of_request15()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort },
                handle => { handle.When("(.*)", "api/(.*)/(.*)/(.*)", request => request.RespondWithString(request.ExtensionlessWithExtension("wooo"))); },
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/path/to/resource.html?name=sam&data=fun").Result;
                    //todo this needs fixing
                    Assert.AreEqual($"http://localhost:{proxyPort}/api/path/to/resource.html.wooo?name=sam&data=fun", result);
                });
        }

        [TestMethod]
        public async Task it_can_remote_proxy()
        {
            int proxyPort = TestHelper.FreeTcpPort();
            int remotePort = TestHelper.FreeTcpPort();
            await TestHelper.Run(
                new List<int>
                    { proxyPort, remotePort },
                handle => handle.RemoteProxyWhenAny($"http://localhost:{remotePort}"),
                (client, servers) =>
                {
                    string result = client.GetStringAsync($"http://localhost:{proxyPort}/api/values/GetAll").Result;
                    Assert.AreEqual(remotePort.ToString(), result);
                });
        }
    }
}