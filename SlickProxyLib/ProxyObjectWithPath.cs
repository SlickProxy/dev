namespace SlickProxyLib
{
    using System;
    using Microsoft.Owin;

    public class ProxyObjectWithPath : ProxyObject
    {
        public ProxyObjectWithPath(OwinAppRequestInformation request, Func<int, string> part, IOwinContext context)
            : base(request, context)
        {
            this.Context = context;
            if (part == null)
                part = i => "";

            this.Part = part;
        }

        IOwinContext Context { get; }

        public Func<int, string> Part { get; internal set; }
    }
}