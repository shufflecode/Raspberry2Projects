using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.BaseClasses
{
  
    internal class ApiController
    {

        public string RouteBase { get; set; }

        public ApiController()
        {
            
        }

        public HttpResponseMessage Ok(object response)
        {
            var responseMessgae = new HttpResponseMessage();
            responseMessgae.StatusCode = HttpStatusCode.OK;
            HttpContent contentPost = new StringContent(response.ToString(), Encoding.UTF8, "application/json");
            responseMessgae.Content = contentPost;
            return responseMessgae;
        }

        public HttpResponseMessage NotFound()
        {
            var responseMessgae = new HttpResponseMessage();
            responseMessgae.StatusCode = HttpStatusCode.NotFound;
            return responseMessgae;
        }

    }
    
}
