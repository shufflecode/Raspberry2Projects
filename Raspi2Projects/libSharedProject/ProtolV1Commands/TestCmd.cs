//#define PC 

//

using libShared;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace libSharedProject.ProtolV1Commands
{
   

    public class TestCmd : ProtocolV1Base
    {
        public TestCmd()
        {
           // this.CollectionProperty = new System.Collections.ObjectModel.Collection<string> { "Item 1", "Item 2", "Item 3" };
        }

        //        /// <summary>
        //        /// RGBA
        //        /// </summary>
        //#if WINDOWS_UWP 
        //#else               
        //        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        //        [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
        //#endif

        //[Editor(typeof(libDesktop.XceedWpfToolkit.LastNameUserControlEditor), typeof(libDesktop.XceedWpfToolkit.LastNameUserControlEditor))]
        //[Category("Information")]
        //[DisplayName("Title Text")]
        //[Description("This property uses a TextBox as the default editor.")]
        public string Title { get; set; } = "Test Class";

        public int I32 { get; set; } = -101;

        public double PI { get; set; } = Math.PI;

        public List<string> TextList { get; set; } = new List<string>() { "Hallo", "Welt" };

        public System.DateTime Datum { get; set; } = System.DateTime.Now;

        //#if WINDOWS_UWP
        //#else
        //        [Newtonsoft.Json.JsonIgnore]
        //        public System.Windows.Media.Color Color1 { get; set; } = System.Windows.Media.Colors.Blue;
        //#endif

#if WINDOWS_UWP
#else
        //[System.ComponentModel.Description("ProtocolV1 Kennung")]
        //[System.ComponentModel.Category("ProtocolV1")]
        //[Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
        //[System.ComponentModel.Editor(typeof(libDesktop.XceedWpfToolkit.UcColorConverter), typeof(libDesktop.XceedWpfToolkit.UcColorConverter))]
        //[System.ComponentModel.Category("Information")]       
        //[System.ComponentModel.Description("This property uses a TextBox as the default editor.")]

        //[System.ComponentModel.Browsable(false)]

        //[System.ComponentModel.Editor(typeof(libDesktop.XceedWpfToolkit.UcColorConverter), typeof(libDesktop.XceedWpfToolkit.UcColorConverter))]
        //[Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
        [System.ComponentModel.Editor(typeof(AppWpfToolkit.UcColorEditor), typeof(AppWpfToolkit.UcColorEditor))]
#endif
        //public libShared.HardwareNah.RGBValue Col1 { get; set; }
        //[Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
        public libShared.SharedColor MyCol { get; set; } = new libShared.SharedColor();

        //[System.ComponentModel.Editor(typeof(libDesktop.XceedWpfToolkit.ReadOnlyCollectionEditor), typeof(libDesktop.XceedWpfToolkit.ReadOnlyCollectionEditor))]
        //public ICollection<string> CollectionProperty { get; private set; }
    }

    public class TestClass : ProtocolV1Base
    {
        public int MyInt { get; set; } = 101;
        public string MyString { get; set; } = "Hallo Welt";
        public double MyDouble { get; set; } = 99.99;
        public DateTime MyDateTime { get; set; } = DateTime.Now;

#if WINDOWS_UWP
#else
        [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
#endif
        public Person Besitzer { get; set; } = new Person() { Alter = 50, FirstName = "Oli", LastName = "d" };


        public List<Person> Personen { get; set; } = new List<Person>();
    }

    public class Person
    {
        public int Alter { get; set; }
        //[Editor(typeof(FirstNameEditor), typeof(FirstNameEditor))]
        public string FirstName { get; set; }

#if WINDOWS_UWP
#else
        [System.ComponentModel.Editor(typeof(AppWpfToolkit.LastNameUserControlEditor), typeof(AppWpfToolkit.LastNameUserControlEditor))]
        [System.ComponentModel.Category("Information")]
        [System.ComponentModel.DisplayName("Last Name")]
        [System.ComponentModel.Description("This property uses a TextBox as the default editor.")]
#endif
        public string LastName { get; set; } = "f";


    }





}
