using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebServer.BaseClasses
{
  
    class ApiController
    {
        public ApiController()
        {
            
        }
        
        public HttpResponseMessage Ok(object response)
        {
            var responseMessgae = new HttpResponseMessage();
            try
            {
                string json = String.Empty;
                DataContractJsonSerializer ser = new DataContractJsonSerializer(response.GetType());
                
                using (MemoryStream ms = new MemoryStream())
                {
                    ser.WriteObject(ms, response);
             
                        json =  Encoding.UTF8.GetString(ms.ToArray());
                }
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
