using libShared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using libCore.UserControls;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace libCore.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PageEthernet : Page
    {

        public PageEthernet()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public PageEthernet(IEthernetAsync _ethernet)    
            : this()        
        {
            this.uc.Ethernet = _ethernet;
            //this.Background = Windows.UI.Colors.Yellow;
        }

        public UcEthernet Uc
        {
            get
            {
                return this.uc;
            }

            set
            {
                this.uc = value;
            }
        }
    }
}
