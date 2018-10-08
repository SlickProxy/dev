[![NuGet version](https://badge.fury.io/nu/slickproxy.svg)](https://badge.fury.io/nu/slickproxy)  For .NETFramework 4.5 and higher


            var settings = new SlickProxySettings
            {
                CaseSensitive = false,
                RouteSameServerRewritesOverNetwork = false
            };

            app.UseSlickProxy(handle =>
                {
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseReferer($"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/boo"), request => request.UseRequestHeaders(headers => headers["Referer"] = $"{request.BaseAddressWithScheme}", request.ExtensionlessWithExtension("html")));
                    handle.When(request => request.Path.EndsWith("/index"), request => request.ExtensionlessWithExtension("html"));
                    handle.When(request => request.Path.EndsWith("/indexString"), request => request.RespondWithString("what's up men!"));
                    handle.When(request => request.Path.EndsWith("/indexObject"), request => request.RespondWithObjectAsJson(DateTime.UtcNow));
                    handle.When("/cdn2/(.*)", request => request.ForwardToDomain("https://code.jquery.com"));
                    handle.When("/cdn/(.*)", "/cdn/(.*)", request => $"{request.Scheme}://code.jquery.com/{request.Part(1)}");

                    handle.When(request => request.QueryStringValueByName("link") == "hello", req => req.Deny());
                    handle.When("(.*)", req => req.Allow());
                    //the matches below will never be reached coz of the allow above
                    handle.DenyAny(HttpStatusCode.BadRequest);
                    handle.When("(.*)", req => req.DenyWith(HttpStatusCode.OK, null));

                    handle.When(request => request.QueryStringContainsName("bad"), req => req.Deny());
                    handle.When("/deny(.*)", request => request.HasNoQueryString(), req => req.Deny());

                    var list = new List<string>
                        { "" };
                    handle.When("/scene(.*)", req => list.Contains(req.QueryStringValueByName("link")), req => $"{req.BaseAddressWithScheme}");
                },settings);
