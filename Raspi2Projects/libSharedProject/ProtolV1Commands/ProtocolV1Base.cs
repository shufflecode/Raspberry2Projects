

using System.ComponentModel;

namespace libSharedProject.ProtolV1Commands
{
    public class ProtocolV1Base
    {
        //[System.ComponentModel.Browsable(false)]
        //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        //[BrowsableAttribute(false), CategoryAttribute("Version"), ReadOnlyAttribute(true)]

        //[Category("ProtocolV1")]        
        //[Description("ProtocolV1 Kennung")]
        public string MyType
        {
            get { return this.GetType().Name; }
        }
    }
}
