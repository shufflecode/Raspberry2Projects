using System.Runtime.Serialization;
using Windows.UI;

namespace WebServer.Models
{
    [DataContract]
    class LEDStatus
    {
        [DataMember]
        public Color color;

        [DataMember]
        public Status status;
        public enum Status
        {
            on=1,
            off=2
        }
    }
}