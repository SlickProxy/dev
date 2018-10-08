namespace SampleHttpsServer
{
    using System;
    using System.Web.Http;

    public class ValuesController : ApiController
    {
        // GET api/values/GetAll
        public object GetAll()
        {
            return DateTime.Now.ToLongDateString();
        }
    }
}