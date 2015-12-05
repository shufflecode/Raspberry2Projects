using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libShared.ApiModels;

namespace WebServer.Models
{
    static class GPIOModel
    {
        public static void SetGpio(GPIOStatus status)
        {
            //mach was 
                    
        }

        public static GPIOStatus GetStatus()
        {
            GPIOStatus status = new GPIOStatus();
            status.Ports.Add(new GPIOPort(1,GPIOPort.Portstatus.high));
            status.Ports.Add(new GPIOPort(2, GPIOPort.Portstatus.high));
            status.Ports.Add(new GPIOPort(3, GPIOPort.Portstatus.high));
            status.Ports.Add(new GPIOPort(4, GPIOPort.Portstatus.low));
            return status;
        }
    }
}
