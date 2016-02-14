using libShared;
using libShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace AppWpfSimpleClient
{
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

        private ConfigFile config = new ConfigFile();

        public ConfigFile ConfigFile
        {
            get { return config; }
            set { config = value; }
        }


        public ObservableCollection<object> CommandList { get; internal set; } = new ObservableCollection<object>();
        public ObservableCollection<object> SendList { get; internal set; } = new ObservableCollection<object>();
        public ObservableCollection<object> ReceiveList { get; internal set; } = new ObservableCollection<object>();

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


        #region Client

        IEthernetSync client = null;

        public IEthernetSync Client
        {
            get
            {
                return client;
            }

            internal set
            {
                client = value;
               
            }
        }

        string port = "27200";

        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                this.OnPropertyChanged();
            }
        }

        string host = "localhost";

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                this.OnPropertyChanged();
            }
        }

        private string valueSendText = "Test Cmd";

        public string ValueSendText
        {
            get { return valueSendText; }
            set
            {
                valueSendText = value;
                this.OnPropertyChanged();
            }
        }

        ObservableCollection<byte> valueSendData = new ObservableCollection<byte>();

        public ObservableCollection<byte> ValueSendData
        {
            get { return valueSendData; }
            set
            {
                valueSendData = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            this.Command = new RelayCommand(this.CommandReceived);

            client = new libDesktop.TcpClientV1();
            Client.NotifyexceptionEvent += Client_NotifyexceptionEvent;
            Client.NotifyTextEvent += Client_NotifyTextEvent;
            Client.NotifyMessageReceivedEvent += Client_NotifyMessageReceivedEvent;
            
            string machineName = Environment.MachineName;

            this.AddInfoTextLine("Hallo Welt");

            for (int i = 0; i < 5; i++)
            {
                CommandList.Add(new TestCmd() { Name = "Comands", Zahl = i });
            }

            for (int i = 0; i < 5; i++)
            {
                SendList.Add(new TestCmd() { Name = "Send", Zahl = i });
            }

            for (int i = 0; i < 5; i++)
            {
                ReceiveList.Add(new TestCmd() { Name = "Receive", Zahl = i });
            }


        }

        private void Client_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data) + " Data:" + Converters.ConvertByteArrayToHexString(data, " "));
        }

        private void Client_NotifyTextEvent(object sender, string text)
        {
            AddInfoTextLine(string.Format("Info from Client: {0}", text));
        }

        private void Client_NotifyexceptionEvent(object sender, Exception ex)
        {
            AddInfoTextLine(string.Format("Exception from Client: {0}", libShared.ExceptionHandling.GetExceptionText(ex)));
        }

        /// <summary>
        /// Empfängt Commands von der Oberfläche
        /// </summary>
        /// <param name="param">Empfangenes Command</param>
        public async void CommandReceived(object param)
        {
            try
            {

                switch (param as string)
                {
                    case "File Open":
                        //DataContractSerializer
                        Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();

                        //if (this.TypeFileDirectory != string.Empty)
                        //{
                        //    openFileDialog1.InitialDirectory = System.IO.Path.Combine(this.TypeFileDirectory, "NMEA");
                        //}

                        openFileDialog1.Filter = "xml files (*.xml)|*.xml";

                        if (openFileDialog1.ShowDialog() == true)
                        {
                            string file = openFileDialog1.FileName;

                            var temp = ConfigFile.Deserialize(file);

                            //this.SelectedCmd = null;
                            //this.CommandList.Clear();
                            //this.SendList.Clear();
                            //this.ReceiveList.Clear();

                            foreach (var item in temp.CommandList)
                            {
                                this.CommandList.Add(item);
                            }

                            foreach (var item in temp.SendList)
                            {
                                SendList.Add(item);
                            }

                            foreach (var item in temp.ReceiveList)
                            {
                                ReceiveList.Add(item);
                            }
                        }

                        break;

                    case "File Save":
                        if (string.IsNullOrEmpty(this.ConfigFile.FileName))
                        {
                            this.CommandReceived("File SaveAs");
                        }
                        else
                        {
                            this.ConfigFile.CommandList = this.CommandList;
                            this.ConfigFile.SendList = this.SendList;
                            this.ConfigFile.ReceiveList = this.ReceiveList;

                            this.ConfigFile.Serialize(this.ConfigFile.FileName);
                        }
                        break;

                    case "File SaveAs":
                        try
                        {
                            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();

                            //if (this.TypeFileDirectory != string.Empty)
                            //{
                            //    saveFileDialog1.InitialDirectory = System.IO.Path.Combine(this.TypeFileDirectory, "NMEA");
                            //}

                            //DataContractSerializer
                            saveFileDialog1.Filter = "xml files (*.xml)|*.xml";

                            if (saveFileDialog1.ShowDialog() == true)
                            {
                                string file = saveFileDialog1.FileName;

                                this.ConfigFile.CommandList = this.CommandList;
                                this.ConfigFile.SendList = this.SendList;
                                this.ConfigFile.ReceiveList = this.ReceiveList;

                                this.ConfigFile.Serialize(file);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        break;

                    case "Start TCP Client":

                        this.Client.HostNameOrIp = this.Host;
                        this.Client.Port = this.Port;
                        this.AddInfoTextLine(string.Format("Client Start Beginn -> Host: {0}, Port: {1}", this.Client.HostNameOrIp, this.Client.Port));
                        this.Client.Start();
                        this.AddInfoTextLine("Client Start: Ende");
                        break;

                    case "Stop TCP Client":
                        this.AddInfoTextLine("Client Stop: Beginn");
                        this.Client.Stop();
                        this.AddInfoTextLine("Client Stop: Ende");
                        break;

                    case "Send TextBlock":
                        this.SendText(this.ValueSendText);
                        break;

                    case "Send Data":
                        this.SendData(this.ValueSendData.ToArray());
                        break;

                    case "Send Selected Command":
                        await this.SendObj(this.SelectedCmd);
                        break;

                    case "Clear Selected Commands":
                        this.CommandList.Clear();
                        break;

                    case "Send All Selected Commands":

                        foreach (var item in this.CommandList)
                        {
                            await this.SendObj(item);
                        }

                        break;

                    case "Send Selected Send":
                        await this.SendObj(this.SelectedCmd);
                        break;

                    case "Clear Send  Commands":
                        this.SendList.Clear();
                        break;

                    case "Send Selected Receive":
                        await this.SendObj(this.SelectedCmd);
                        break;

                    case "Clear Receive Commands":
                        this.ReceiveList.Clear();
                        break;

                    default:
                        this.AddInfoTextLine(string.Format("Command {0} not Implemented", param.ToString()));
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public async Task SendObj(object obj)
        {
            this.SendList.Add(obj);
        }


        public void SendText(string text)
        {
            try
            {
                if (this.Client != null)
                {
                    this.Client.SendText(text);
                }
                else
                {
                    this.AddInfoTextLine("Client Not Running");
                }
            }
            catch (Exception ex)
            {
                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (this.Client != null)
                {
                    this.Client.SendData(data);
                }
                else
                {
                    this.AddInfoTextLine("Client Not Running");
                }
            }
            catch (Exception ex)
            {
                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        /// <summary>
        /// Hilfs Methode für den ScrollViewr, damit dieser Automatisch an das Ende scrollt.
        /// </summary>
        /// <param name="sender">Sender objcect</param>
        /// <param name="e">ScrollChanged EventArgs</param>
        protected void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
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

        private void ListBox_Selected(object sender, RoutedEventArgs e)
        {
           
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var l = sender as ListBox;

            if (l.SelectedItem != null)
            {
                SendObj(l.SelectedItem);
            }            
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                this.SelectedCmd = e.AddedItems[0];
            }           
        }
    }

    public class TestCmd
    {
        private int zahl;

        public int Zahl
        {
            get { return zahl; }
            set { zahl = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return string.Format("Name: {0}; Zahl: {1}", Name, Zahl);
        }

    }
}
