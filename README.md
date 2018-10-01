[![NuGet version](https://badge.fury.io/nu/slickproxy.svg)](https://badge.fury.io/nu/slickproxy)


            app.UseSlickProxy(
                request =>
                {
                    request.When(req => req.Path.EndsWith("/boo"), req => req.UseReferer($"{req.BaseAddressWithScheme}", req.ExtensionlessWithExtension("html")));
                    request.When(req => req.Path.EndsWith("/boo"), req => req.UseRequestHeaders(headers => headers["Referer"] = $"{req.BaseAddressWithScheme}" , req.ExtensionlessWithExtension("html")));
                    request.When(req => req.Path.EndsWith("/index"), req => req.ExtensionlessWithExtension("html"));
                    request.When(req => req.Path.EndsWith("/indexString"), req => req.RespondWithString("what's up men!"));
                    request.When(req => req.Path.EndsWith("/indexObject"), req => req.RespondWithObjectAsJson(DateTime.UtcNow));

                    request.When("/cdn2/(.*)", req => req.ForwardToDomain("https://code.jquery.com"));

                    request.When("/cdn/(.*)", "/cdn/(.*)", req => $"{req.Scheme}://code.jquery.com/{req.Part(1)}");

                    //request.When(req => req.Scheme == "http", req => req.RedirectTo(req.AsHttps));

                    request.OnRewriteStarted((f, t) => Console.WriteLine($"Started from {f} to {t} ..."));
                    request.OnRedirect((f, t) => Console.WriteLine($"Redirecting from {f} to {t} ..."));
                    request.OnRewriteEnded((f, t) => Console.WriteLine($"Ended from {f} to {t} ..."));
                    request.OnRewriteToCurrentHost((f, t) => Console.WriteLine($"Rewritten to current server from {f} to {t} ..."));
                    request.OnRewriteException((f, r, e) => Console.WriteLine($"Error when rewriting from {f} gave error ..." + e));
                });
