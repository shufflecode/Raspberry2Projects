//#define PC 

using libShared;
using System;
using System.Collections.Generic;

namespace libSharedProject.ProtolV1Commands
{
    public class TestCmd : ProtocolV1Base
    {
        /// <summary>
        /// RGBA
        /// </summary>
#if (PC)
        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
#endif
        public SharedColor Col1 { get; set; } = new SharedColor() { R = 0xF0 };

        public string Title { get; set; } = "Test Class";

        public int I32 { get; set; } = -101;

        public double PI { get; set; } = Math.PI;

        public List<string> TextList { get; set; } = new List<string>() { "Hallo", "Welt" };

        public System.DateTime Datum { get; set; } = System.DateTime.Now;        
    }
}
