[![NuGet version](https://badge.fury.io/nu/slickproxy.svg)](https://badge.fury.io/nu/slickproxy)


            app.UseSlickProxy(
                request =>
                {
                    request.When(req => request.Uri.EndsWith("/index"), req => $"{req.BaseAddressWithScheme}/index.html");
                    request.When("/cdn/(.*)", "/cdn/(.*)", req => $"{req.Scheme}://code.jquery.com/{req.Part(1)}");
                    request.OnRewriteStarted((f, t) => Console.WriteLine($"Started from {f} to {t} ..."));
                    request.OnRewriteEnded((f, t) => Console.WriteLine($"Ended from {f} to {t} ..."));
                    request.OnRewriteException((f, r, e) => Console.WriteLine($"Error when rewriting from {f} gave error ..." + e));
                });
