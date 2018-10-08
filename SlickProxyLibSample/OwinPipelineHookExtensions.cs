using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace SlickProxyLibSample
{
    using Owin;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class OwinPipelineHookExtensions
    {
        public static IAppBuilder UseHooks(
            this IAppBuilder app,
            Action<IDictionary<string, object>> before = null,
            Action<IDictionary<string, object>> after = null)
        {
            return app.Use(
                new Func<AppFunc, AppFunc>(
                    next => async env =>
                    {
                        if (before != null)
                            before.Invoke(env);

                        await next.Invoke(env);

                        if (after != null)
                            after.Invoke(env);
                    }));
        }

        public static IAppBuilder UseHooks<TState>(
            this IAppBuilder app,
            Func<IDictionary<string, object>, TState> before = null,
            Action<TState, IDictionary<string, object>> after = null,
            TState defaultState = default(TState))
        {
            return app.Use(
                new Func<AppFunc, AppFunc>(
                    next => async env =>
                    {
                        TState state = defaultState;

                        if (before != null)
                            state = before.Invoke(env);

                        await next.Invoke(env);

                        if (after != null)
                            after.Invoke(state, env);
                    }));
        }

        public static IAppBuilder UseHooksAsync(
            this IAppBuilder app,
            Func<IDictionary<string, object>, Task> before = null,
            Func<IDictionary<string, object>, Task> after = null)
        {
            return app.Use(
                new Func<AppFunc, AppFunc>(
                    next => async env =>
                    {
                        if (before != null)
                            await before.Invoke(env);

                        await next.Invoke(env);

                        if (after != null)
                            await after.Invoke(env);
                    }));
        }

        public static IAppBuilder UseHooksAsync<TState>(
            this IAppBuilder app,
            Func<IDictionary<string, object>, Task<TState>> before = null,
            Func<TState, IDictionary<string, object>, Task> after = null,
            TState defaultState = default(TState))
        {
            return app.Use(
                new Func<AppFunc, AppFunc>(
                    next => async env =>
                    {
                        TState state = defaultState;

                        if (before != null)
                            state = await before.Invoke(env);

                        await next.Invoke(env);

                        if (after != null)
                            await after.Invoke(state, env);
                    }));
        }
    }
}