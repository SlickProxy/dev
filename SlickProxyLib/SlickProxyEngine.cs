namespace SlickProxyLib
{
    using Microsoft.Owin;
    using Newtonsoft.Json;
    using Owin;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public static class SlickProxyEngine
    {
        //todo try flurl https://www.nuget.org/packages/Flurl.Http/
        private static HttpClient client { set; get; }

        public static IAppBuilder UseSlickProxy(this IAppBuilder app, Action<OwinAppRequestInformation> when, SlickProxySettings settings = null)
        {
            //requestInfo.CancellationToken;
            //From https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            //Windows will hold a connection in this state for 240 seconds
            //(It is set by [HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\TcpTimedWaitDelay]).
            //There is a limit to how quickly
            //Windows can open new sockets so if you exhaust the connection pool then you’re likely to see error
            // In this case disposing of HttpClient was the wrong thing to do.
            // It is unfortunate that HttpClient implements IDisposable and encourages the wrong behaviour
            //Also from http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html
            //Using Singleton HttpClient results in your instance not to honour DNS changes which can have serious implications.
            //The solution is to set the ConnectionLeaseTimeout of the ServicePoint object for the endpoint.

            ServicePointManager.DefaultConnectionLimit = 100;
            client = new HttpClient();

            //client.DefaultRequestHeaders.ConnectionClose  will set the HTTP’s keep-alive header to false so
            //the socket will be closed
            //after a single request. It turns out this can add roughly extra 35ms
            //(with long tails, i.e amplifying outliers) to each of your HTTP calls preventing you to take advantage of benefits of re-using a socket. So what is the solution then?
            // client.DefaultRequestHeaders.ConnectionClose = true;
            //var sp = ServicePointManager.FindServicePoint(new Uri("http://foo.bar/baz/123?a=ab"));
            //sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute

            settings = settings ?? new SlickProxySettings();
            settings.@this = new OwinAppRequestInformation(null, settings);
            app.Use(
                new Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>(
                    next => async env =>
                    {
                        IOwinContext context = new OwinContext(env);
                        var requestInfo = new OwinAppRequestInformation(context, settings);

                        string from = requestInfo.Uri;
                        HttpRequestMessage request = null;
                        try
                        {
                            when(requestInfo);

                            if (requestInfo.ProxyObject.Forwarding != null)
                                requestInfo.RewriteToUrl = $"{requestInfo.ProxyObject.Forwarding}{requestInfo.PathAndQuery}";

                            if (requestInfo.RequireAuthentication && !(context.Authentication.User.Identities.FirstOrDefault()?.IsAuthenticated ?? false))
                            {
                                requestInfo.Settings.OnRouteBlocking?.Invoke(from);
                                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                                response.Content = new StringContent(HttpStatusCode.Unauthorized.ToString());
                                HttpContent stream = response.Content;
                                context.Response.StatusCode = (int)response.StatusCode;
                                await stream.CopyToAsync(requestInfo.ResponseBody);
                            }
                            else if (requestInfo.ProxyObject.ContinueToOtherOwinPipeline)
                            {
                                requestInfo.OnAllowedToContinue?.Invoke(from);
                                await next.Invoke(env);
                            }
                            else if (requestInfo.ProxyObject.BlockRequestWith != null)
                            {
                                requestInfo.Settings.OnRouteBlocking?.Invoke(from);
                                var response = new HttpResponseMessage(requestInfo.ProxyObject.BlockRequestWith.Item1);
                                string message = requestInfo.ProxyObject.BlockRequestWith.Item2 ?? "";
                                response.Content = new StringContent(message);
                                HttpContent stream = response.Content;
                                context.Response.StatusCode = (int)response.StatusCode;
                                await stream.CopyToAsync(requestInfo.ResponseBody);
                            }
                            else if (requestInfo.ProxyObject.RedirectToString != null)
                            {
                                requestInfo.Settings.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.RedirectToString, requestInfo.Method);
                                context.Response.Redirect(requestInfo.ProxyObject.RedirectToString);
                            }
                            else if (requestInfo.ProxyObject.ReturnString != null)
                            {
                                requestInfo.Settings.OnRedirectTo?.Invoke(from, requestInfo.ProxyObject.ReturnString, requestInfo.Method);
                                context.Response.Write(requestInfo.ProxyObject.ReturnString);
                            }
                            else if (requestInfo.ProxyObject.ReturnObjectAsJson != null)
                            {
                                string json = JsonConvert.SerializeObject(requestInfo.ProxyObject.ReturnObjectAsJson);
                                requestInfo.Settings.OnRedirectTo?.Invoke(from, json, requestInfo.Method);
                                context.Response.Write(json);
                            }
                            else if (!string.IsNullOrEmpty(requestInfo.RewriteToUrl))
                            {
                                request = OwinRequestToHttpRequestMessage(requestInfo);
                                
                              

                                bool rewriteToSameServer = requestInfo.RewriteToUrl.ToLower().StartsWith(context.Request.Uri.GetLeftPart(UriPartial.Authority).ToLower());
                                requestInfo.Settings.OnRewritingStarted?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method, rewriteToSameServer);
                                var myUri = new Uri(requestInfo.RewriteToUrl);

                                if (rewriteToSameServer && !requestInfo.Settings.RouteSameServerRewritesOverNetwork)
                                {
                                    requestInfo.Settings.OnRewriteToCurrentServer?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method);
                                    string newPath = myUri.PathAndQuery;
                                    context.Request.Path = new PathString(newPath);
                                    requestInfo.Settings.OnProcessingEnded?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method);
                                    await next.Invoke(env);
                                }
                                else
                                {
                                    //https://github.com/dotnet/corefx/issues/11224
                                    //Now to fix it, all we need to do is to get hold of the ServicePoint object for
                                    //the endpoint by passing the URL to it and set the ConnectionLeaseTimeout:
                                    // http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html ConnectionLeaseTimeout which controls how many milliseconds a TCP socket should
                                    // be kept open. Its default value is -1 which means connections will be stay open
                                    // indefinitely… well in real terms, until the server closes the connection or there
                                    // is a network disruption - or the HttpClientHandler gets disposed as discussed.
                                    ServicePoint sp = ServicePointManager.FindServicePoint(myUri);
                                    sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute

                                    requestInfo.Settings.OnRewriteToDifferentServer?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method);

                                    ServicePointManager.SecurityProtocol = 
                                        //SecurityProtocolType.Tls |
                                        SecurityProtocolType.Ssl3 |
                                        SecurityProtocolType.Tls12;

                                    HttpResponseMessage response = await request.SendAsync( requestInfo.Method,from, requestInfo.RewriteToUrl, requestInfo.Settings.OnRewritingException, requestInfo.Settings.OnRespondingFromRemoteServer);
                                    context.Response.StatusCode = (int)response.StatusCode;
                                    if (!string.IsNullOrEmpty(requestInfo.ResponseContentHeadersContentType)) {
                                        context.Response.ContentType = requestInfo.ResponseContentHeadersContentType;
                                    }
                                    else{
                                        context.Response.ContentType = response.Content.Headers.ContentType.MediaType;
                                    }
                                       
                                    await response.Content.CopyToAsync(requestInfo.ResponseBody);

                                    if (requestInfo.Settings.CollectRequestResponse != null)
                                    {
                                        var  requestCopy = OwinRequestToHttpRequestMessage(requestInfo);
                                        HttpResponseMessage responseCopy = await requestCopy.SendAsync( requestInfo.Method,from, requestInfo.RewriteToUrl, requestInfo.Settings.OnRewritingException, requestInfo.Settings.OnRespondingFromRemoteServer);
                                        requestInfo.Settings.CollectRequestResponse.Invoke(new ResponseInspection(from, requestInfo.RewriteToUrl, responseCopy.StatusCode, context.Response.ContentType, responseCopy.Content));
                                    }

                                    requestInfo.Settings.OnProcessingEnded?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method);
                                }
                            }
                            else
                            {
                                requestInfo.Settings.OnNoMatching?.Invoke(from);
                                await next.Invoke(env);
                            }
                        }
                        catch (Exception e)
                        {
                            requestInfo.Settings.OnRewritingException?.Invoke(from, requestInfo.RewriteToUrl, requestInfo.Method, request ?? OwinRequestToHttpRequestMessage(requestInfo), e);
                            throw;
                        }
                    }));
            return app;
        }

        //based on https://github.com/petermreid/buskerproxy/blob/master/BuskerProxy/Handlers/ProxyHandler.cs
        public static async Task<HttpResponseMessage> SendAsync(this HttpRequestMessage request,string method, string from, string remote, Action<string, string,string, HttpRequestMessage, Exception> requestInfoOnRewritingException, Action<string, string,string, HttpRequestMessage, HttpResponseMessage, Exception, string> requestInfoOnRespondingFromRemoteServer)
        {
            try
            {
                //Trace.TraceInformation("Request To:{0}", request.RequestUri.ToString());
                HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote,method, request, response, null, "Request succeeded");
                response.Headers.Via.Add(new ViaHeaderValue("1.1", "SignalXProxy", "http"));
                //same again clear out due to protocol violation
                if (request.Method == HttpMethod.Head)
                    response.Content = null;

                return response;
            }
            catch (HttpRequestException e)
            {
                string errorMessage = e.Message;
                if (e.InnerException != null)
                    errorMessage += " - " + e.InnerException.Message;

                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote,method, request, null, e, "Request failed");
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(errorMessage)
                };
            }
            catch (PlatformNotSupportedException e)
            {
                // For instance, on some OSes, .NET Core doesn't yet
                // support ServerCertificateCustomValidationCallback

                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote,method, request, null, e, "Sorry, your system does not support the requested feature.");
                return new HttpResponseMessage
                {
                    StatusCode = 0,
                    Content = new StringContent(e.Message)
                };
            }
            catch (TaskCanceledException e)
            {
                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote,method, request, null, e, " The request timed out, the endpoint might be unreachable.");

                return new HttpResponseMessage
                {
                    StatusCode = 0,
                    Content = new StringContent(e.Message + " The endpoint might be unreachable.")
                };
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                //context.Response.StatusCode = (int)response.StatusCode;
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += ':' + ex.InnerException.Message;
                response.Content = new StringContent(message);
                Trace.TraceError("Error:{0}", message);
                requestInfoOnRespondingFromRemoteServer?.Invoke(from, remote,method, request, response, ex, "Request failed");
                return response;
            }
        }

        private static HttpRequestMessage OwinRequestToHttpRequestMessage(OwinAppRequestInformation requestInfo)
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
            return request;
        }
    }
}