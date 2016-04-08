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
using System.Reflection;

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

        string protocolV1Kennung;

        string reflectionText = string.Empty;

        public string ReflectionText
        {
            get
            {
                return reflectionText;
            }

            internal set
            {
                reflectionText = value;
                this.OnPropertyChanged();
            }
        }

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

        public System.Data.DataView CommandView { get; private set; }
        public System.Data.DataView SendView { get; private set; }
        public System.Data.DataView ReceiveView { get; private set; }

        public DataSet1.DataTableCmdDataTable CommandTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();
        public DataSet1.DataTableCmdDataTable SendTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();
        public DataSet1.DataTableCmdDataTable ReceiveTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();

        //public ObservableCollection<KeyValuePair<string, object>> CommandList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();
        //public ObservableCollection<KeyValuePair<string, object>> SendList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();
        //public ObservableCollection<KeyValuePair<string, object>> ReceiveList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();

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
        libSharedProject.ProtolV1Commands.ProtocolV1Base fd = new libSharedProject.ProtolV1Commands.ProtocolV1Base();

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

            var b = new libSharedProject.ProtolV1Commands.ProtocolV1Base();
            protocolV1Kennung = this.GetPropertyName(() => b.MyType);

            client = new libDesktop.TcpClientV1();
            Client.NotifyexceptionEvent += Client_NotifyexceptionEvent;
            Client.NotifyTextEvent += Client_NotifyTextEvent;
            Client.NotifyMessageReceivedEvent += Client_NotifyMessageReceivedEvent;

            string machineName = Environment.MachineName;

            this.AddInfoTextLine("Hallo Welt");


            //libSharedProject.ProtolV1Commands.TestClass c = new libSharedProject.ProtolV1Commands.TestClass();
            //this.SelectedCmd = c;

            //c.Title += "dsf";
            //for (int i = 0; i < 5; i++)
            //{
            //    //CommandList.Add(new libShared.ProtolV1Commands.TestCmd() { Title = "Comand " + i.ToString() });
            //    CommandList.Add(new TestCmd());// { Title = "Comand " + i.ToString() });
            //}

            //for (int i = 0; i < 5; i++)
            //{
            //    SendList.Add(new TestCmd() { Title = "Send " + i.ToString() });
            //}

            //for (int i = 0; i < 5; i++)
            //{
            //    ReceiveList.Add(new TestCmd() { Title = "Comand " + i.ToString() });
            //}

            this.CommandView = this.CommandTable.DefaultView;
            this.SendView = this.SendTable.DefaultView;
            this.ReceiveView = this.ReceiveTable.DefaultView;


            Assembly asem = fd.GetType().Assembly;
            Type[] types = asem.GetTypes(); //Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type t in types)
            {
                if (t.BaseType != null && t.BaseType.Equals(typeof(libSharedProject.ProtolV1Commands.ProtocolV1Base)))
                {
                    //AddInfoTextLine(t.Name);
                    //object ob = Activator.CreateInstance(t);
                    //CommandList.Add(new KeyValuePair<string, object>("CMD", ob));
                    ////CommandList2.Add(new KeyValuePair<string, object>("CMD", ob));
                    ////int x = 0;

                    //var nr = this.CommandTable.NewDataTableCmdRow();
                    //nr.Info = t.Name;
                    //nr.JSON = Newtonsoft.Json.JsonConvert.SerializeObject(ob);
                    ////nr.Comand = ob;
                    //this.CommandTable.AddDataTableCmdRow(nr);                  


                    RunActionMenuItem btn = new RunActionMenuItem(t.Name, () => AddCmd(t));
                    this.CmdMenuItems.Add(btn);
                }
            }
        }

        public void AddCmd(Type t)
        {
            object obx = Activator.CreateInstance(t);
            var newRow = this.CommandTable.NewDataTableCmdRow();
            newRow.Info = t.Name;
            newRow.JSON = Newtonsoft.Json.JsonConvert.SerializeObject(obx);
            this.CommandTable.AddDataTableCmdRow(newRow);
        }

        /// <summary>
        /// Erweiterter Button der eine im übergebene Methode ohne Returnwert (inklusive Parameter) ausführt.
        /// </summary>
        public class RunActionMenuItem : System.Windows.Controls.MenuItem
        {
            private Action actionToRun;

            /// <summary>
            /// Konstruktor der Klasse
            /// </summary>
            /// <param name="text">Beschrifftung des Buttons</param>
            /// <param name="methodToRun">Action (Methode ohne Returnwert inklusive Parameter) welche bei Button Click ausgeführt werden soll</param>
            /// RunActionButton btn = new RunActionButton("Test Achse Nr.:X", () => MethodenName(Parameter,...));
            public RunActionMenuItem(string text, Action methodToRun)
            {
                this.actionToRun = methodToRun;
                this.Click += RunActionButton_Click;
                this.Header = text;
                //this.Content = text;
            }

            private void RunActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                this.actionToRun();
            }
        }


        public System.Windows.Controls.MenuItem CreateMenuItem(string content, string commandParameter)
        {
            System.Windows.Controls.MenuItem btn = new System.Windows.Controls.MenuItem();
            btn.Command = this.Command;
            btn.Header = content;
            btn.CommandParameter = commandParameter;
            return btn;
        }

        /// <summary>
        /// Get the name of a static or instance property from a property access lambda.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        /// <returns>The name of the property</returns>
        private string GetPropertyName<T>(System.Linq.Expressions.Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as System.Linq.Expressions.MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }

        private void Client_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            try
            {
                string ret = Encoding.UTF8.GetString(data);

                var obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(ret);

                if (obj.GetValue("MyType").ToString() == nameof(libSharedProject.ProtolV1Commands.TestCmd))
                {
                    libSharedProject.ProtolV1Commands.TestCmd dfdf = (libSharedProject.ProtolV1Commands.TestCmd)obj.ToObject(typeof(libSharedProject.ProtolV1Commands.TestCmd));

                    var newRow = this.ReceiveTable.NewDataTableCmdRow();
                    newRow.Info = obj.GetValue("MyType").ToString();
                    newRow.TimeStamp = DateTime.Now;
                    newRow.JSON = ret;
                    this.ReceiveTable.AddDataTableCmdRow(newRow);

                    //this.ReceiveList.Add(new KeyValuePair<string, object>(string.Format("RX: {0}", System.DateTime.Now), dfdf));
                }
                else
                {
                    this.AddInfoTextLine("Text:" + ret + " Data:" + Converters.ConvertByteArrayToHexString(data, " "));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
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

                        openFileDialog1.Filter = "xml files (*.xml)|*.xml";//  "json files (*.json)|*.json";//;

                        if (openFileDialog1.ShowDialog() == true)
                        {
                            string file = openFileDialog1.FileName;

                            var temp = ConfigFile.Deserialize(file);

                            //this.SelectedCmd = null;
                            //this.CommandList.Clear();
                            //this.SendList.Clear();
                            //this.ReceiveList.Clear();

                            //foreach (var item in temp.CommandList2)
                            //{
                            //    this.CommandList2.Add(item);
                            //}

                            //foreach (var item in temp.CommandList)
                            //{
                            //    this.CommandList.Add(item);
                            //}

                            //foreach (var item in temp.SendList)
                            //{
                            //    SendList.Add(item);
                            //}

                            //foreach (var item in temp.ReceiveList)
                            //{
                            //    ReceiveList.Add(item);
                            //}

                            foreach (var item in temp.CommandTable.Rows)
                            {
                                var newRow = this.CommandTable.NewDataTableCmdRow();
                                newRow.ItemArray = (object[])((System.Data.DataRow)item).ItemArray.Clone();
                                this.CommandTable.AddDataTableCmdRow(newRow);
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
                            this.ConfigFile.CommandTable = this.CommandTable;
                            this.ConfigFile.SendTable = this.SendTable;
                            this.ConfigFile.ReceiveTable = this.ReceiveTable;

                            //this.ConfigFile.CommandList = this.CommandList;
                            //this.ConfigFile.SendList = this.SendList;
                            //this.ConfigFile.ReceiveList = this.ReceiveList;

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
                            saveFileDialog1.Filter = "xml files (*.xml)|*.xml";// "json files (*.json)|*.json";// ;

                            if (saveFileDialog1.ShowDialog() == true)
                            {
                                string file = saveFileDialog1.FileName;

                                this.ConfigFile.CommandTable = this.CommandTable;
                                this.ConfigFile.SendTable = this.SendTable;
                                this.ConfigFile.ReceiveTable = this.ReceiveTable;

                                //this.ConfigFile.CommandList = this.CommandList;
                                //this.ConfigFile.SendList = this.SendList;
                                //this.ConfigFile.ReceiveList = this.ReceiveList;

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
                        await this.SendCmd(this.SelectedRow);
                        break;

                    case "Create Json":
                        string json = CreateJsonString(this.SelectedCmd);



                        this.AddInfoTextLine(json);
                        break;

                    case "Clear Selected Commands":
                        this.CommandTable.Rows.Clear();
                        //this.CommandList.Clear();
                        break;

                    case "Send All Selected Commands":

                        foreach (var item in this.CommandTable.Rows)
                        {
                            DataSet1.DataTableCmdRow row = (DataSet1.DataTableCmdRow)item;
                            await this.SendCmd(row);

                        }

                        //foreach (var item in this.CommandList)
                        //{
                        //    await this.SendObj(item.Value);
                        //}



                        break;

                    //case "Send Selected Send":
                    //    //await this.SendObj(this.SelectedCmd);
                    //    await this.SendCmd(this.SelectedRow);
                    //    break;

                    //case "Clear Send  Commands":
                    //    //this.SendList.Clear();
                    //    this.SendTable.Rows.Clear();
                    //    break;

                    //case "Send Selected Receive":
                    //    await this.SendObj(this.SelectedCmd);
                    //    break;

                    //case "Clear Receive Commands":
                    //    //this.ReceiveList.Clear();
                    //    this.ReceiveTable.Rows.Clear();
                    //    break;

                    //case "Send SelectedObject":
                    //    await this.SendObj(this.SelectedCmd);
                    //    break;

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

        public string CreateJsonString(object obj)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
                return string.Empty;
            }
        }

        public async Task SendCmd(DataSet1.DataTableCmdRow row)
        {
            var newSendRow = this.SendTable.NewDataTableCmdRow();
            newSendRow.ItemArray = (object[])(row.ItemArray.Clone());
            newSendRow.TimeStamp = DateTime.Now;
            this.SendTable.AddDataTableCmdRow(newSendRow);

            this.SendText(row.JSON);
        }

        //public async Task SendObj(object obj)
        //{
        //    try
        //    {
        //        string json = CreateJsonString(obj);
        //        this.SendText(json);

        //        this.AddInfoTextLine(json);

        //        var newRow = this.SendTable.NewDataTableCmdRow();
                
        //        newRow.TimeStamp = DateTime.Now;
        //        newRow.JSON = json;
        //        this.SendTable.AddDataTableCmdRow(newRow);

        //        //this.SendList.Add(new KeyValuePair<string, object>(string.Format("TX: {0}", System.DateTime.Now), obj));
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    }
        //}        

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

        //private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var l = sender as ListBox;

        //    if (l.SelectedItem != null)
        //    {
        //        SendObj(((KeyValuePair<string, object>)l.SelectedItem).Value);
        //    }
        //}

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                this.SelectedCmd = ((KeyValuePair<string, object>)e.AddedItems[0]).Value;
            }
        }

        //private void ListBox_SelectionChangedCmd(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
        //    {
        //        this.SelectedCmd = e.AddedItems[0];
        //    }
        //}

        // PropertyValueChangedEventHandler



        private void PropertyGrid_SelectedPropertyItemChanged(object sender, RoutedPropertyChangedEventArgs<Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemBase> e)
        {
            //if (e != null && e.NewValue != null)
            //{
            //    bool isExpandable = e.NewValue.IsExpandable;
            //    object editor = e.NewValue.Editor;
            //    string cat = ((Xceed.Wpf.Toolkit.PropertyGrid.CustomPropertyItem)e.NewValue).Category;

            //    object instance = ((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.NewValue).Instance;
            //    string name = e.NewValue.DisplayName;
            //    object value = ((Xceed.Wpf.Toolkit.PropertyGrid.CustomPropertyItem)e.NewValue).Value;

            //    if (value != null)
            //    {
            //        Type valueType = value.GetType();

            //        bool isClass = valueType.IsClass;
            //        bool isValueType = valueType.IsValueType;
            //        bool isPrimitive = valueType.IsPrimitive;
            //        string names = valueType.Namespace;

            //        bool isStruct = value.GetType().IsValueType && !value.GetType().IsEnum;

            //        if (isStruct && string.IsNullOrEmpty(names) == false && names.StartsWith("System") == false)
            //        {
            //            //// eigenes struct
            //            if (e.NewValue.IsExpandable == false)
            //            {
            //                e.NewValue.IsExpandable = true;
            //            }
            //        }
            //    }
            //}
        }

        private void PropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid grid = sender as Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid;
            OnPropertyChanged(((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemBase)e.OriginalSource).DisplayName);

            if (this.SelectedRow != null)
            {
                this.SelectedRow.JSON = Newtonsoft.Json.JsonConvert.SerializeObject(this.SelectedCmd);
            }
        }

        private ObservableCollection<object> cmdMenuItems = new ObservableCollection<object>();

        public ObservableCollection<object> CmdMenuItems
        {
            get
            {
                return cmdMenuItems;
            }            
        }


        DataSet1.DataTableCmdRow selectedRow;

        public DataSet1.DataTableCmdRow SelectedRow
        {
            get
            {
                return selectedRow;
            }

            internal set
            {
                selectedRow = value;
                this.OnPropertyChanged();
            }
        }

        

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.SelectedRow = (DataSet1.DataTableCmdRow)((System.Data.DataRowView)e.AddedItems[0]).Row;

                var obj = libSharedProject.ProtolV1Commands.ProtocolV1Base.ConvertJsonStingToObj(this.SelectedRow.JSON);
                this.ReflectionText = GetInfo(obj);

                this.SelectedCmd = obj;

                //var obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(this.SelectedRow.JSON);

                //if (obj.GetValue(this.protocolV1Kennung).ToString() == nameof(libSharedProject.ProtolV1Commands.TestCmd))
                //{
                //    this.SelectedCmd = (libSharedProject.ProtolV1Commands.TestCmd)obj.ToObject(typeof(libSharedProject.ProtolV1Commands.TestCmd));
                //}
                //else
                //{
                //    this.SelectedCmd = obj;
                //}
            }
            catch (Exception ex)
            {
                this.AddInfoTextLine(ex.Message);
            }

        }

        public string GetInfo(object obj)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                Type type = obj.GetType();
                object[] customAttributes = type.GetCustomAttributes(true);
                for (int i = 0; i < (int)customAttributes.Length; i++)
                {
                    object attribut = customAttributes[i];
                    if (attribut.GetType().Equals(typeof(DescriptionAttribute)))
                    {
                        sb.AppendLine(string.Format("{0}", ((DescriptionAttribute)attribut).Description));
                    }
                }

                if ((sb == null ? false : sb.Length > 0))
                {
                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }



        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{

        //    AppWpfSimpleClient.DataSet1 dataSet1 = ((AppWpfSimpleClient.DataSet1)(this.FindResource("dataSet1")));
        //}

        //private void Window_Loaded_1(object sender, RoutedEventArgs e)
        //{

        //    AppWpfSimpleClient.DataSet1 dataSet1 = ((AppWpfSimpleClient.DataSet1)(this.FindResource("dataSet1")));
        //}
    }


}
