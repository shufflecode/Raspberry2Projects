using System;

using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;


namespace WebServer.BaseClasses
{
  
    class ApiController
    {
        public ApiController()
        {
            
        }

        /// <summary>
        /// Empty HTTP 200 OK Message
        /// </summary>
        /// <returns> Empty HTTP 200 OK Message</returns>
        public HttpResponseMessage Ok()
        {
            var responseMessgae = new HttpResponseMessage();
            responseMessgae.StatusCode = HttpStatusCode.OK;
            responseMessgae.Content = new StringContent(string.Empty);
            return responseMessgae;
        }

        /// <summary>
        /// HTTP 200 OK Message 
        /// </summary>
        /// <param name="response">Object to pass to the client</param>
        /// <returns>HttpResponse OK with JSONB Formatted Data</returns>
        public HttpResponseMessage Ok(object response)
        {
            var responseMessgae = new HttpResponseMessage();
            try
            {
                string json = String.Empty;
                json = JsonConvert.SerializeObject(response);
          
                responseMessgae.StatusCode = HttpStatusCode.OK;
                
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                responseMessgae.Content = contentPost;
            }
            catch (Exception ex)
            {
                responseMessgae.StatusCode = HttpStatusCode.InternalServerError;
                responseMessgae.Content = new StringContent("Exception thrown"+ ex,Encoding.ASCII);
            }
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
