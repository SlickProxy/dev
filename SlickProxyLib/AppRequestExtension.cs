namespace SlickProxyLib
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public static class AppRequestExtension
    {
        public static void When(this OwinAppRequestInformation @this, Func<ProxyObjectWithPath, bool> test, Func<ProxyObject, string> apply)
        {
            if (@this.IsMatched)
                return;

            if (test(@this.ProxyObject))
            {
                string to = apply(@this.ProxyObject);
                @this.RewriteToUrl = to;
                @this.IsMatched = true;
            }
        }

        /// <summary>
        ///     Apply in any case
        /// </summary>
        /// <param name="apply"></param>
        public static void WhenAny(this OwinAppRequestInformation @this, Func<ProxyObject, string> apply)
        {
            @this.When("(.*)", apply);
        }

        public static void RemoteProxyWhenAny(this OwinAppRequestInformation @this, string baseAddressWithPortAndScheme)
        {
            @this.WhenAny(request => baseAddressWithPortAndScheme.TrimEnd('/').TrimEnd('\\') + request.PathAndQuery);
        }

        public static void RemoteProxyWithExtensionWhenNoPath(this OwinAppRequestInformation @this, string baseAddressWithPortAndScheme, string extension, string contentType = null)
        {
            @this.WhenNoPath(
                request =>
                {
                    request.SetResponseContentType("text/html");
                    return baseAddressWithPortAndScheme.TrimEnd('/').TrimEnd('\\') + "/" + request.PartWithSourceAndPattern(request.ExtensionlessWithExtension(extension), "//(.*)/(.*)", 2);
                });
        }

        /// <summary>
        ///     Apply only if regex matches
        /// </summary>
        /// <param name="regexMatch"></param>
        /// <param name="apply"></param>
        public static void When(this OwinAppRequestInformation @this, string regexMatch, Func<ProxyObject, string> apply)
        {
            @this.When(regexMatch, regexMatch, apply);
        }

        /// <summary>
        ///     Apply only if test expression resolves to true
        /// </summary>
        /// <param name="test"></param>
        /// <param name="apply"></param>
        public static void When(this OwinAppRequestInformation @this, Func<ProxyObjectWithPath, bool> test, Action<ProxyObject> apply)
        {
            if (@this.IsMatched)
                return;

            if (test(@this.ProxyObject))
            {
                apply(@this.ProxyObject);
                @this.IsMatched = true;
            }
        }

        /// <summary>
        ///     Apply if regex matches. You can extract parts of the second expression into the third
        /// </summary>
        /// <param name="regexMatch"></param>
        /// <param name="extractionString"></param>
        /// <param name="apply"></param>
        public static void When(this OwinAppRequestInformation @this, string regexMatch, string extractionString, Func<ProxyObjectWithPath, string> apply)
        {
            if (@this.IsMatched)
                return;

            Match match = @this.Settings.CaseSensitive ? Regex.Match(@this.Uri, regexMatch) : Regex.Match(@this.Uri, regexMatch, RegexOptions.IgnoreCase);
            Match rep = @this.Settings.CaseSensitive ? Regex.Match(@this.Uri, extractionString) : Regex.Match(@this.Uri, extractionString, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                @this.ProxyObject = new ProxyObjectWithPath(@this, i => rep.Groups[i].Value, @this.Context);
                string to = apply(@this.ProxyObject);
                @this.RewriteToUrl = to;
                @this.IsMatched = true;
            }
        }

        /// <summary>
        ///     Apply if  test expression resolves to true and regex matches. You can extract parts of the second expression into
        ///     the third
        /// </summary>
        /// <param name="regexMatch"></param>
        /// <param name="test"></param>
        /// <param name="apply"></param>
        public static void When(this OwinAppRequestInformation @this, string regexMatch, Func<ProxyObjectWithPath, bool> test, Func<ProxyObjectWithPath, string> apply)
        {
            if (@this.IsMatched)
                return;

            Match match = @this.Settings.CaseSensitive ? Regex.Match(@this.Uri, regexMatch) : Regex.Match(@this.Uri, regexMatch, RegexOptions.IgnoreCase);
            if (match.Success && test(@this.ProxyObject))
            {
                string to = apply(@this.ProxyObject);
                @this.RewriteToUrl = to;
                @this.IsMatched = true;
            }
        }

        /// <summary>
        ///     Apply if  test expression resolves to true. You can extract parts of the second expression into the third
        /// </summary>
        /// <param name="test"></param>
        /// <param name="extractionString"></param>
        /// <param name="apply"></param>
        public static void When(this OwinAppRequestInformation @this, Func<ProxyObjectWithPath, bool> test, string extractionString, Func<ProxyObjectWithPath, string> apply)
        {
            if (@this.IsMatched)
                return;

            Match rep = @this.Settings.CaseSensitive ? Regex.Match(@this.Uri, extractionString) : Regex.Match(@this.Uri, extractionString, RegexOptions.IgnoreCase);
            if (test(@this.ProxyObject))
            {
                @this.ProxyObject = new ProxyObjectWithPath(@this, i => rep.Groups[i].Value, @this.Context);
                string to = apply(@this.ProxyObject);
                @this.RewriteToUrl = to;
                @this.IsMatched = true;
            }
        }

        public static void DenyAny(this OwinAppRequestInformation @this, HttpStatusCode? httpStatusCode = null, string message = null)
        {
            @this.When("(.*)", req => httpStatusCode == null ? req.Deny() : req.DenyWith(httpStatusCode.Value, message));
        }

        public static void WhenNoPath(this OwinAppRequestInformation @this, string extractionString, Func<ProxyObjectWithPath, string> apply)
        {
            @this.When(req => req.Path == "/" || req.Path == "" || req.Path == "\\", extractionString, apply);
        }

        public static void WhenNoPath(this OwinAppRequestInformation @this, Func<ProxyObjectWithPath, string> apply)
        {
            @this.When(req => req.Path == "/" || req.Path == "" || req.Path == "\\", "(.*)", apply);
        }
    }
}