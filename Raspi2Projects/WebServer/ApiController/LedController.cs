using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using WebServer.Models;

namespace WebServer.ApiController
{
    class LedController : BaseClasses.ApiController
    {
        public LedController()
        {
        }

        [Route("/LedController/Green")]
        public HttpResponseMessage LedGreen(int LEDNumber)
        {
            return Ok(new LEDStatus()
            {
               status = LEDStatus.Status.on,
               color = Colors.Green
            });
        }


        [Route("/LedController/Blue")]
        public HttpResponseMessage LedBlue(int LEDNumber)
        {
            return Ok(new LEDStatus()
            {
                status = LEDStatus.Status.on,
                color = Colors.Blue
            });
        }
    }
}
