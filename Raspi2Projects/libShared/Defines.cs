using System;

namespace libShared
{
    public delegate void NotifyTextDelegate(object sender, string text);
    public delegate void NotifyexceptionDelegate(object sender, Exception ex);
    public delegate void NotifyMessageReceivedDelegate(object sender, byte[] data);
    public delegate void NotifyConnectionClosed(object sender);
}
