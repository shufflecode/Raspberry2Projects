
using System.Net.Http;

using Windows.UI;
using BackgroundWebserver.Models;

namespace BackgroundWebserver.ApiController
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
