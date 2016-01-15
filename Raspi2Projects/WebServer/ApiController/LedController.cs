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
        private LEDDemo demo;
        public LedController()
        {
            demo = new LEDDemo();
        }

        [Route("/LedController/Demo/on", Route.Type.Get)]
        public HttpResponseMessage DemoON(int time)
        {
            demo.StartTimer(time);
            return Ok("Gestartet ;) ");
        }

        [Route("/LedController/Demo/off", Route.Type.Get)]
        public HttpResponseMessage DemoOff()
        {
            demo.StopDemo();
            return Ok("RGB Demo aus");
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
