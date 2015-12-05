using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCore
{
    public delegate void NotifyTextDelegate(object sender, string text);
    public delegate void NotifyexceptionDelegate(object sender, Exception ex);
    public delegate void NotifyMessageReceivedDelegate(object sender, byte[] data);
}
