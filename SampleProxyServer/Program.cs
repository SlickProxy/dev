namespace SampleProxyServer
{
    using Microsoft.Owin.Hosting;
    using System;
    using System.Diagnostics;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string httpLocalhost = "http://localhost:9900";
            string query = "http://localhost:9900/api/values/getall";
            using (WebApp.Start<StartUp>(httpLocalhost))
            {
                Console.WriteLine("Press [enter] to quit...");
                Process.Start(httpLocalhost);
                Console.ReadLine();
            }
        }
    }
}