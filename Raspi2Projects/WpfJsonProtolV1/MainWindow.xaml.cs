//using libSharedProject.ProtolV1Commands;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace WpfJsonProtolV1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        TestClass cl = new TestClass();

        public MainWindow()
        {
            InitializeComponent();

            cl.Personen.Add(new Person() { Alter = 15, FirstName = "Alf", LastName = "" });
            cl.Personen.Add(new Person() { Alter = 35, FirstName = "Franka", LastName = "" });

            pgrid1.SelectedObject = cl;
        }

        private object selectedCmd;

        public object SelectedCmd
        {
            get { return selectedCmd; }
            set
            {
                selectedCmd = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyname">Name der Property welche sich geändert hat.</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class TestClass
    {
        public int MyInt { get; set; } = 101;
        public string MyString { get; set; } = "Hallo Welt";
        public double MyDouble { get; set; } = 99.99;
        public DateTime MyDateTime { get; set; } = DateTime.Now;

        [ExpandableObject]
        public Person Besitzer { get; set; } = new Person() { Alter = 50, FirstName = "Oli", LastName = "d" };


        public List<Person> Personen { get; set; } = new List<Person>();
    }

    public class Person
    {
        public int Alter { get; set; }
        //[Editor(typeof(FirstNameEditor), typeof(FirstNameEditor))]
        public string FirstName { get; set; }

        [Editor(typeof(libDesktop.XceedWpfToolkit.LastNameUserControlEditor), typeof(libDesktop.XceedWpfToolkit.LastNameUserControlEditor))]
        //[Editor(typeof(WpfJsonProtolV1.LastNameUserControlEditor), typeof(WpfJsonProtolV1.LastNameUserControlEditor))]
        [Category("Information")]
        [DisplayName("Last Name")]
        [Description("This property uses a TextBox as the default editor.")]
        public string LastName { get; set; } = "f";


    }
}
