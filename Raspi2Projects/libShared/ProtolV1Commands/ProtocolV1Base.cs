using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared.ProtolV1Commands
{
    public class ProtocolV1Base
    {
        public string MyType
        {
            get { return this.GetType().Name; }
        }

        //public string Title { get; set; } = "CMD";
    }
}
