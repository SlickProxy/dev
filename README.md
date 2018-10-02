[![NuGet version](https://badge.fury.io/nu/slickproxy.svg)](https://badge.fury.io/nu/slickproxy)

            app.UseSlickProxy(
                handle =>
                {
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseReferer($"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseRequestHeaders(headers => headers["Referer"] = $"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/index"), request => request.ExtensionlessWithExtension("html"));
                    handle.When(request => request.Path.EndsWith("/indexString"), request => request.RespondWithString("what's up men!"));
                    handle.When(request => request.Path.EndsWith("/indexObject"), request => request.RespondWithObjectAsJson(DateTime.UtcNow));
                    handle.When("/cdn2/(.*)", request => request.ForwardToDomain("https://code.jquery.com"));
                    handle.When("/cdn/(.*)", "/cdn/(.*)", request => $"{request.Scheme}://code.jquery.com/{request.Part(1)}");
                    //requestuest.When(request => request.Scheme == "http", request => request.RedirectTo(request.AsHttps));
                    handle.OnRewriteStarted((from, to, sameServer) => { Console.WriteLine($"Started from {from} to {to} ..."); });
                    handle.OnRedirect((from, to) => { Console.WriteLine($"Redirecting from {from} to {to} ..."); });
                    handle.OnRewriteEnded((from, to) => { Console.WriteLine($"Ended from {from} to {to} ..."); });
                    handle.OnRewriteToCurrentHost((from, to) => { Console.WriteLine($"Rewritten to current server from {from} to {to} ..."); });
                    handle.OnRewriteException((from, to, requestMessage, error) => { Console.WriteLine($"Error when rewriting from {from} to {to} gave error ..." + error); });
                    handle.OnResponseFromRemoteServer((from, to, requestMessage, responseMessage, exception) => { Console.WriteLine($"When rewriting from {from} to {to} gave error ..." + exception + $" content {responseMessage.Content.ReadAsStringAsync().Result}"); });
                    handle.OnNoMatch(from => { Console.WriteLine($"No match for  {from} "); });
                });
