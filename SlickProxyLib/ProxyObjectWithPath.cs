namespace SlickProxyLib
{
    using Microsoft.Owin;
    using System;
    using System.Text.RegularExpressions;

    public class ProxyObjectWithPath : ProxyObject
    {
        public ProxyObjectWithPath(OwinAppRequestInformation request, Func<int, string> part, IOwinContext context)
            : base(request, context)
        {
            this.Context = context;
            if (part == null)
                part = i => "";

            this.Part = part;
            this.PartWithPattern = (p, i) =>
            {
                Match rep = request.Settings.CaseSensitive ? Regex.Match(request.Uri, p) : Regex.Match(request.Uri, p, RegexOptions.IgnoreCase);
                return rep.Success ? rep.Groups[i].Value : "";
            };
            this.PartWithSourceAndPattern = (s, p, i) =>
            {
                Match rep = request.Settings.CaseSensitive ? Regex.Match(s, p) : Regex.Match(s, p, RegexOptions.IgnoreCase);
                return rep.Success ? rep.Groups[i].Value : "";
            };
        }

        private IOwinContext Context { get; }

        /// <summary>
        ///     Returns the matched string from the previous pattern based on the request uri
        /// </summary>
        public Func<int, string> Part { get; internal set; }

        /// <summary>
        ///     Supply a pattern to get the matched string based on the request uri
        /// </summary>
        public Func<string, int, string> PartWithPattern { get; internal set; }

        /// <summary>
        ///     Supply a string and the matching pattern to get the matched string based on the string you provide
        /// </summary>
        public Func<string, string, int, string> PartWithSourceAndPattern { get; internal set; }
    }
}