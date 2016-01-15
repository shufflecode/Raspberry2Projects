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

        public HttpResponseMessage Ok()
        {
            var responseMessgae = new HttpResponseMessage();
            responseMessgae.StatusCode = HttpStatusCode.OK;
            responseMessgae.Content = new StringContent(string.Empty);
            return responseMessgae;
        }

        /// <summary>
        /// ApiMeldung OK mit entsprechend Formatiertem Json Object
        /// </summary>
        /// <param name="response">Das Objekt das ausgegeben werden soll</param>
        /// <returns>HttpResponse OK mit Json Daten</returns>
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
                var msg = ex.Message;
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
