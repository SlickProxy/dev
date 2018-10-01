namespace SlickProxyLib
{
    using System;

    public class ProxyObjectWithPath : ProxyObject
    {
        public ProxyObjectWithPath(OwinAppRequestInformation request, Func<int, string> part)
            : base(request)
        {
            if (part == null)
                part = i => "";

            this.Part = part;
        }

        public Func<int, string> Part { get; internal set; }

       
    }
}