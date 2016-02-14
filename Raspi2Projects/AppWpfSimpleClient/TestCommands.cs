using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AppWpfSimpleClient
{
    //[Serializable, DataContract]
    public class TestCmd : libShared.ProtolV1Commands.ProtocolV1Base
    {
        //[DataMember]
        public string Title { get; set; } = "Test Class";

        //[DataMember]
        public int I32 { get; set; } = -1;

        //[DataMember]
        public double PI { get; set; } = Math.PI;

        //[DataMember]
        public List<string> TextList { get; set; } = null;

        //[DataMember]
        public System.DateTime Datum { get; set; } = System.DateTime.Now;        

        [Browsable(false)]
        [Newtonsoft.Json.JsonIgnore]
        libShared.SharedColor sharedColor = new libShared.SharedColor();

        [Newtonsoft.Json.JsonIgnore]
        System.Windows.Media.Color mediaColor  = System.Windows.Media.Colors.Red;

        [Newtonsoft.Json.JsonIgnore]
        public Color MediaColor
        {
            get
            {
                return mediaColor;
            }

            set
            {
                mediaColor = value;
                sharedColor.A = mediaColor.A;
                sharedColor.R = mediaColor.R;
                sharedColor.B = mediaColor.B;
                sharedColor.G = mediaColor.G;                
            }
        }

        public libShared.SharedColor SharedColor
        {
            get
            {
                return sharedColor;
            }

            set
            {
                sharedColor = value;
                mediaColor.A = sharedColor.A;
                mediaColor.R = sharedColor.R;
                mediaColor.B = sharedColor.B;
                mediaColor.G = sharedColor.G;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
