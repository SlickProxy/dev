namespace SlickProxyLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Newtonsoft.Json;
    using Owin;

    public static class SlickProxyEngine
    {
        public static void UseSlickProxy(this IAppBuilder app, Action<OwinAppRequestInformation> when)
        {
            app.Use(
                new Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>(
                    next => async env =>
                    {
                        IOwinContext context = new OwinContext(env);
                        var requestInfo = new OwinAppRequestInformation(context);

                        string from = requestInfo.Uri;
                        try
                        {
                            when(requestInfo);

                            if (requestInfo.ProxyObject.Forwarding != null)
                                requestInfo.RewriteToUrl = $"{requestInfo.ProxyObject.Forwarding}{requestInfo.PathAndQuery}";

                            if (requestInfo.ProxyObject.RedirectToString != null)
                            {
                                requestInfo.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.RedirectToString);
                                context.Response.Redirect(requestInfo.ProxyObject.RedirectToString);
                                return;
                            }
                            else if (requestInfo.ProxyObject.ReturnString != null)
                            {
                                requestInfo.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.ReturnString);
                                context.Response.Write(requestInfo.ProxyObject.ReturnString);
                                return;
                            }
                            else if (requestInfo.ProxyObject.ReturnObjectAsJson != null)
                            {
                                string json = JsonConvert.SerializeObject(requestInfo.ProxyObject.ReturnObjectAsJson);
                                requestInfo.OnRedirectTo?.Invoke(from, json);
                                context.Response.Write(json);
                                return;
                            }
                            else if (!string.IsNullOrEmpty(requestInfo.RewriteToUrl))
                            {
                                requestInfo.OnRewritingStarted?.Invoke(from, requestInfo.RewriteToUrl);

                                var myUri = new Uri(requestInfo.RewriteToUrl);

                                bool rewriteToSameServer = myUri.Host + ":" + myUri.Port == requestInfo.HostName + ":" + requestInfo.Port && (
                                    requestInfo.RewriteToUrl.StartsWith("http://" + requestInfo.HostNameWithPort + requestInfo.Path) ||
                                    requestInfo.RewriteToUrl.StartsWith("https://" + requestInfo.HostNameWithPort + requestInfo.Path)
                                );

                                if (rewriteToSameServer)
                                {
                                    requestInfo.OnRewriteToCurrentServer?.Invoke(from, requestInfo.RewriteToUrl);

                                    string newPath = myUri.PathAndQuery;
                                    context.Request.Path = new PathString(newPath);
                                    requestInfo.OnRewritingEnded?.Invoke(from, requestInfo.RewriteToUrl);
                                    await next.Invoke(env);
                                }
                                else
                                {
                                    HttpContent stream = await requestInfo.SendAsync(from, requestInfo.RewriteToUrl, requestInfo.OnRewritingException);
                                    await stream.CopyToAsync(requestInfo.ResponseBody);
                                    requestInfo.OnRewritingEnded?.Invoke(from, requestInfo.RewriteToUrl);
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            requestInfo.OnRewritingException?.Invoke(from, requestInfo, e);
                            throw;
                        }

                        await next.Invoke(env);
                    }));
        }

        //based on https://github.com/petermreid/buskerproxy/blob/master/BuskerProxy/Handlers/ProxyHandler.cs
        public static async Task<HttpContent> SendAsync(this OwinAppRequestInformation requestInfo, string from, string remote, Action<string, OwinAppRequestInformation, Exception> requestInfoOnRewritingException)
        {
            requestInfo.RewriteToUrl = remote;
            //requestInfo.CancellationToken;
            string clientIp = requestInfo.RemoteIpAddress;

            HttpRequestMessage request = OwinRequestToHttpRequestMessage(requestInfo);

            var client = new HttpClient();
            try
            {
                request.Headers.Add("X-Forwarded-For", clientIp);
                //Trace.TraceInformation("Request To:{0}", request.RequestUri.ToString());
                HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.Headers.Via.Add(new ViaHeaderValue("1.1", "SignalXProxy", "http"));
                //same again clear out due to protocol violation
                if (request.Method == HttpMethod.Head)
                    response.Content = null;
                return response.Content;
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += ':' + ex.InnerException.Message;
                response.Content = new StringContent(message);
                Trace.TraceError("Error:{0}", message);
                requestInfoOnRewritingException?.Invoke(from, requestInfo, ex);
                return response.Content;
            }
        }

        static HttpRequestMessage OwinRequestToHttpRequestMessage(OwinAppRequestInformation requestInfo)
        {
            var method = new HttpMethod(requestInfo.Method);

            var request = new HttpRequestMessage(method, requestInfo.RewriteToUrl);

            //have to explicitly null it to avoid protocol violation
            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Trace) request.Content = null;

            //now check if the request came from our secure listener then outgoing needs to be secure
            if (request.Headers.Contains("X-Forward-Secure"))
            {
                request.RequestUri = new UriBuilder(request.RequestUri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri;
                request.Headers.Remove("X-Forward-Secure");
            }

            return request;
        }
    }
}