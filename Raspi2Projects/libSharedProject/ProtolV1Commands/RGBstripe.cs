using System;
using System.Collections.Generic;
using System.Text;

namespace libSharedProject.ProtolV1Commands
{
    public class RGBstripeColor : ProtocolV1Base
    {
#if WPF_TOOLKIT
        [System.ComponentModel.Editor(typeof(AppWpfToolkit.UcColorEditor), typeof(AppWpfToolkit.UcColorEditor))]
#endif
        public libShared.SharedColor StripeSingleColor { get; set; } = new libShared.SharedColor();
    }
}