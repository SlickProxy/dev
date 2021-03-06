﻿namespace SlickProxyLibSample
{
    using Microsoft.Owin.Hosting;
    using System;
    using System.Diagnostics;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const string httpLocalhost = "http://localhost:9000";
            using (WebApp.Start<StartUp>(httpLocalhost))
            {
                Console.WriteLine("Opening a browser to " + httpLocalhost);
                Console.WriteLine("Press [enter] to quit...");
                Process.Start(httpLocalhost);
                Console.ReadLine();
            }
        }
    }
}