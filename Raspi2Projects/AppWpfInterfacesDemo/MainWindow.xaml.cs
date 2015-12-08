namespace AppWpfInterfacesDemoV1
{
    using libShared;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Hilfs Varibale
        /// </summary>
        private bool autoScroll = true;

        /// <summary>
        /// Multi Threading Hilfs Objekt.
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        /// <summary>
        /// Dispatcher Hilf Objekt.
        /// Stellt Dienste zum Verwalten der Warteschlange von Arbeitsaufgaben für einen Thread bereit.
        /// </summary>
        public virtual System.Windows.Threading.Dispatcher DispatcherObject { get; protected set; }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        private ObservableCollection<object> infoTextList = new ObservableCollection<object>();


        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public ObservableCollection<object> InfoTextList
        {
            get
            {
                return this.infoTextList;
            }
        }


        private ObservableCollection<object> controlCollection1 = new ObservableCollection<object>();



        public ObservableCollection<object> ControlCollection1
        {
            get
            {
                return controlCollection1;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                View = this,
            };

            this.DataContext = DataContext;

            this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            this.Command = new RelayCommand(this.CommandReceived);


            //System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
            //client.Connect("192.168.23", 27200);


            //this.ControlCollection1.Add(new MyObject { Title = "Collection Item 1" });
            //this.ControlCollection1.Add(new MyObject { Title = "Collection Item 2" });
            //this.ControlCollection1.Add(this.CreateMenuItem("Hallo", Commands.ShowInfo.ToString()));

            //// Zeichenfolgenarray mit den Befehlszeilenargumenten für das Programm
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            //// Programm Version Ermitteln
            Version softwareVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //// Benutzer Name ermitteln
            string userName = Environment.UserName;

            //// Programm Name ermitteln
            string softwareName = string.Format("{0}", System.Windows.Application.ResourceAssembly.GetName().Name);

            //// Datei Version Ermitteln
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string softwareDateiVersion = fileVersionInfo.ProductVersion;

            //// Das aktuelle Arbeitsverzeichnisses ermitteln
            string currentDirectory = Environment.CurrentDirectory;

            ////
            string machineName = Environment.MachineName;
        }

        public class MyObject
        {
            public string Title { get; set; }
        }

        public System.Windows.Controls.MenuItem CreateMenuItem(string content, string commandParameter)
        {
            System.Windows.Controls.MenuItem btn = new System.Windows.Controls.MenuItem();
            btn.Command = this.Command;
            btn.Header = content;
            btn.CommandParameter = commandParameter;
            return btn;
        }

        public System.Windows.Controls.Button CreateButton(string content, string commandParameter)
        {
            System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
            btn.Command = this.Command;
            btn.Content = content;
            btn.CommandParameter = commandParameter;
            return btn;
        }

        /// <summary>
        /// Empfängt Commands von der Oberfläche
        /// </summary>
        /// <param name="param">Empfangenes Command</param>
        public void CommandReceived(object param)
        {
            try
            {

                switch (param as string)
                {
                    case "Show TCP Server":

                        if (true)
                        {
                            Window window = new Window
                            {
                                Content = new libDesktop.UserControls.UcEthernet(new libDesktop.TcpServerV1()),
                                Height = 1130
                            };

                            window.Show();
                        }


                        break;

                    case "Show TCP Client":

                        if (true)
                        {
                            Window window = new Window
                            {
                                Content = new libDesktop.UserControls.UcEthernet(new libDesktop.TcpClientV1()),
                                Height = 1130
                            };

                            window.Show();
                        }

                        break;

                    case "Show RS232":

                        if (true)
                        {
                            Window window = new Window
                            {
                                //Title = "EZ/553B PIC ED/526, ED/470",
                                Content = new libDesktop.UserControls.UcSerial(),
                                Height = 1130
                            };

                            window.Show();
                        }

                        break;

                    default:
                        this.AddInfoTextLine(string.Format("Command {0} not Implemented", param.ToString()));
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        /// <summary>
        /// Hilfs Methode für den ScrollViewr, damit dieser Automatisch an das Ende scrollt.
        /// </summary>
        /// <param name="sender">Sender objcect</param>
        /// <param name="e">ScrollChanged EventArgs</param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                //// User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0)
                {
                    //// Content unchanged : user scroll event
                    if (((ScrollViewer)sender).VerticalOffset == ((ScrollViewer)sender).ScrollableHeight)
                    {
                        //// Scroll bar is in bottom
                        //// Set autoscroll mode
                        this.autoScroll = true;
                    }
                    else
                    {   //// Scroll bar isn't in bottom
                        //// Unset autoscroll mode
                        this.autoScroll = false;
                    }
                }

                //// Content scroll event : autoscroll eventually
                if (this.autoScroll && e.ExtentHeightChange != 0)
                {
                    //// Content changed and autoscroll mode set
                    //// Autoscroll
                    ((ScrollViewer)sender).ScrollToVerticalOffset(((ScrollViewer)sender).ExtentHeight);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
        }

        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(object sender, string line)
        {
            try
            {
                if (this.DispatcherObject.Thread != System.Threading.Thread.CurrentThread)
                {
                    this.DispatcherObject.Invoke(new Action(() => this.AddInfoTextLine(sender, line)));
                }
                else
                {
                    lock (this.lockThis)
                    {
                        System.Windows.Controls.TextBlock block = new System.Windows.Controls.TextBlock();
                        block.Text = line;
                        this.InfoTextList.Add(block);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
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
}
