namespace SlickProxyLibTestServerLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    public class ControllerResolver : DefaultHttpControllerTypeResolver
    {
        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(i => typeof(IHttpController).IsAssignableFrom(i)).ToList();
        }
    }
}