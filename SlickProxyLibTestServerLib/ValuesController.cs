namespace SlickProxyLibTestServerLib
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Newtonsoft.Json;

    public class ValuesController : ApiController
    {
        internal static Dictionary<int, Dictionary<HttpMethod, Dictionary<string, RequestConstruct>>> RequestResponseDefinition = new Dictionary<int, Dictionary<HttpMethod, Dictionary<string, RequestConstruct>>>();

        Task<HttpResponseMessage> Process(Func<RequestConstruct> operation, HttpRequestMessage request)
        {
            RequestConstruct result = operation();
            return Task.FromResult(request.CreateResponse(result.ResponseStatusCode, result.Response));
        }

        // GET api/<controller>
        public Task<HttpResponseMessage> GetAll()
        {
            return this.Process(() => RequestResponseDefinition[this.Request.RequestUri.Port][HttpMethod.Get][""], this.Request);
        }

        // GET api/<controller>/5
        public Task<HttpResponseMessage> Get(dynamic id)
        {
            return this.Process(() => RequestResponseDefinition[this.Request.RequestUri.Port][HttpMethod.Get][JsonConvert.SerializeObject(id)], this.Request);
        }

        // POST api/<controller>
        public Task<HttpResponseMessage> Post([FromBody] dynamic value)
        {
            return this.Process(() => RequestResponseDefinition[this.Request.RequestUri.Port][HttpMethod.Post][JsonConvert.SerializeObject(value)], this.Request);
        }

        // PUT api/<controller>/5
        public Task<HttpResponseMessage> Put(int id, [FromBody] string value)
        {
            return this.Process(() => RequestResponseDefinition[this.Request.RequestUri.Port][HttpMethod.Put][JsonConvert.SerializeObject(value)], this.Request);
        }

        // DELETE api/<controller>/5
        public Task<HttpResponseMessage> Delete(int id)
        {
            return this.Process(() => RequestResponseDefinition[this.Request.RequestUri.Port][HttpMethod.Delete][JsonConvert.SerializeObject(id)], this.Request);
        }
    }
}