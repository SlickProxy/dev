namespace SampleHttpsServer
{
    using Microsoft.Owin.Hosting;
    using System;
    using System.Diagnostics;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string httpLocalhost = "https://localhost:44305";
            string query = "https://localhost:44305/api/values/getall";
            using (WebApp.Start<StartUp>(httpLocalhost))
            {
                Console.WriteLine("Press [enter] to quit...");
                Process.Start(query);
                Console.ReadLine();
            }
        }
    }
}