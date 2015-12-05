using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared.ApiModels
{
    public class GPIOStatus
    {
        public List<GPIOPort> Ports;

        public GPIOStatus()
        {
            this.Ports = new List<GPIOPort>();
        }
    }

    public class GPIOPort
    {
        public int Portnumber { get; set; }
        public Portstatus status { get; set; }
        
        public GPIOPort(int portnumber, Portstatus status)
        {
            this.status = status;
            this.Portnumber = portnumber;
        }
        public enum Portstatus
        {
            high =1,
            low =0,
        }
    }
}
