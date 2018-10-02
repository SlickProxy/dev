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
                        HttpRequestMessage request = null;
                        try
                        {
                            when(requestInfo);

                            if (requestInfo.ProxyObject.Forwarding != null)
                                requestInfo.RewriteToUrl = $"{requestInfo.ProxyObject.Forwarding}{requestInfo.PathAndQuery}";

                            if (requestInfo.ProxyObject.RedirectToString != null)
                            {
                                requestInfo.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.RedirectToString);
                                context.Response.Redirect(requestInfo.ProxyObject.RedirectToString);
                            }
                            else if (requestInfo.ProxyObject.ReturnString != null)
                            {
                                requestInfo.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.ReturnString);
                                context.Response.Write(requestInfo.ProxyObject.ReturnString);
                            }
                            else if (requestInfo.ProxyObject.ReturnObjectAsJson != null)
                            {
                                string json = JsonConvert.SerializeObject(requestInfo.ProxyObject.ReturnObjectAsJson);
                                requestInfo.OnRedirectTo?.Invoke(from, json);
                                context.Response.Write(json);
                            }
                            else if (!string.IsNullOrEmpty(requestInfo.RewriteToUrl))
                            {
                                request = OwinRequestToHttpRequestMessage(requestInfo);

                                string clientIp = requestInfo.RemoteIpAddress;
                                request.Headers.Add("X-Forwarded-For", clientIp);
                                if (requestInfo.ProxyObject.Referer != null)
                                    requestInfo.ProxyObject.RequestHeadersChanges.Add("Referer", requestInfo.ProxyObject.Referer);

                                foreach (KeyValuePair<string, string> keyValuePair in requestInfo.ProxyObject.RequestHeadersChanges)
                                    if (!string.IsNullOrEmpty(keyValuePair.Key) && !string.IsNullOrEmpty(keyValuePair.Value))
                                    {
                                        if (request.Headers.Contains(keyValuePair.Key))
                                            request.Headers.Remove(keyValuePair.Key);
                                        request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                                    }

                                bool rewriteToSameServer = requestInfo.RewriteToUrl.ToLower().StartsWith(context.Request.Uri.GetLeftPart(UriPartial.Authority).ToLower());
                                requestInfo.OnRewritingStarted?.Invoke(from, requestInfo.RewriteToUrl, rewriteToSameServer);
                                var myUri = new Uri(requestInfo.RewriteToUrl);
                                if (rewriteToSameServer)
                                {
                                    requestInfo.OnRewriteToCurrentServer?.Invoke(from, requestInfo.RewriteToUrl);

                                    string newPath = myUri.PathAndQuery;
                                    context.Request.Path = new PathString(newPath);
                                    requestInfo.OnProcessingEnded?.Invoke(from, requestInfo.RewriteToUrl);
                                    await next.Invoke(env);
                                }
                                else
                                {
                                    requestInfo.OnRewriteToDifferentServer?.Invoke(from, requestInfo.RewriteToUrl);

                                    HttpContent stream = await request.SendAsync(from, requestInfo.RewriteToUrl, requestInfo.OnRewritingException, requestInfo.OnRespondingFromRemoteServer);
                                    await stream.CopyToAsync(requestInfo.ResponseBody);
                                    requestInfo.OnProcessingEnded?.Invoke(from, requestInfo.RewriteToUrl);
                                }
                            }
                            else
                            {
                                requestInfo.OnNoMatching?.Invoke(from);
                                await next.Invoke(env);
                            }
                        }
                        catch (Exception e)
                        {
                            requestInfo.OnRewritingException?.Invoke(from, requestInfo.RewriteToUrl, request ?? OwinRequestToHttpRequestMessage(requestInfo), e);
                            throw;
                        }
                    }));
        }

        //based on https://github.com/petermreid/buskerproxy/blob/master/BuskerProxy/Handlers/ProxyHandler.cs
        public static async Task<HttpContent> SendAsync(this HttpRequestMessage request, string from, string remote, Action<string, string, HttpRequestMessage, Exception> requestInfoOnRewritingException, Action<string, string, HttpRequestMessage, HttpResponseMessage, Exception> requestInfoOnRespondingFromRemoteServer)
        {
            //requestInfo.CancellationToken;

            var client = new HttpClient();
            try
            {
                //Trace.TraceInformation("Request To:{0}", request.RequestUri.ToString());
                HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote, request, response, null);
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
                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote, request, response, ex);
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