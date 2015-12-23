using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using libShared.ApiModels;
using WebServer.Models;

namespace WebServer.ApiController
{
    class GPIOController:BaseClasses.ApiController
    {
        [Route("/GPIO/",Route.Type.Get)]
        public HttpResponseMessage SetPortStatus(int portNumber)
        {
            return Ok(new GPIOPort(1,GPIOPort.Portstatus.low));
        }

        [Route("/GPIO/SetGlobal",Route.Type.Post)]
        public HttpResponseMessage SetGPIOStatus(GPIOStatus status)
        {
            GPIOModel.SetGpio(status);
            return Ok();
        }

        [Route("/GPIO/GetGlobal", Route.Type.Get)]
        public HttpResponseMessage GetGPIOStatus()
        {
            var status = GPIOModel.GetStatus();
            return Ok(status);
        }
    }
}
