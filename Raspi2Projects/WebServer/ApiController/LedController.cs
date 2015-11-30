using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.ApiController
{
    class LedController : BaseClasses.ApiController
    {
        public LedController()
        {
        }

        [Route("/LedController/LED")]
        public HttpResponseMessage Led(int LEDNumber)
        {
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("LED Controller Methode LED")
            };
        }

        public override string ToString()
        {
            return "LED - Controller";
        }

    }
}
