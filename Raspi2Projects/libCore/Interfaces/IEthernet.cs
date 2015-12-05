using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCore.Interfaces
{
    public interface IEthernet
    {
        event NotifyTextDelegate NotifyTextEvent;
        event NotifyexceptionDelegate NotifyexceptionEvent;
        event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;
        bool IsConnected { get; }
        string HostNameOrIp { get; set; }
        string Port { get; set; }
    }

    public interface IEthernetSync : IEthernet
    {
        void Start();
        void Stop();
        void SendData(byte[] data);
        void SendText(string text);
    }

    public interface IEthernetAsync : IEthernet
    {
        Task Start();
        Task Stop();
        Task SendData(byte[] data);
        Task SendText(string text);
    }
}
