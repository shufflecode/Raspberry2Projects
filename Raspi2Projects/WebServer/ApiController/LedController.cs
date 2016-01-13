using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using libShared.ApiModels;
using WebServer.Models.LedModels;

namespace WebServer.ApiController
{
    class LedController : BaseClasses.ApiController
    {
        public LedController()
        {
        }

        [Route("/LedController/RGBDemo",Route.Type.Get)]
        public HttpResponseMessage LedGreen( )
        {
            var demo = new LEDDemo();
            demo.Start();

            return Ok(new LEDStatus()
            {
                LedNumber = 2,
                status = LEDStatus.Status.on
            });
        }

        [Route("/LedController/Blue",Route.Type.Get)]
        public HttpResponseMessage LedBlue( )
        {
            return Ok(new LEDStatus()
            {
                LedNumber = 1,
                status = LEDStatus.Status.on
            });
        }
    }
}
