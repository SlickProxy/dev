namespace SlickProxyLib
{
    using System;

    public class ProxyObject
    {
        public string Scheme { get; set; }

        public string BaseAddress { get; set; }

        public string Path { get; set; }

        public Func<int, string> Part { get; set; }

        public string Method { get; set; }

        public string Protocol { get; set; }

        public string QueryString { get; set; }

        /// <summary>
        ///     Port not implemented at the moment
        /// </summary>
        public string Port { get; set; }
    }
}