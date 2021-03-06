﻿using libShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using libShared.Interfaces;
using Windows.Networking.Connectivity;
using System.Threading;
using libSharedProject.ProtolV1Commands;
//using libSharedProject.ProtolV1Commands;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace AppSimpleServer
{


    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        // Create an event to signal the timeout count threshold in the
        // timer callback.
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        Timer cylictimer;
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        private ObservableCollection<string> infoboxCollection = new ObservableCollection<string>();

        private string infoText = string.Empty;

        public string InfoText
        {
            get
            {
                return infoText;
            }

            set
            {
                infoText = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> InfoTextList { get; internal set; }

        #region Server Ethernet Handling

        libShared.Interfaces.IEthernetAsync server;

        public IEthernetAsync Server
        {
            get
            {
                return server;
            }

            internal set
            {
                server = value;
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

        ObservableCollection<string> hostNames = new ObservableCollection<string>();

        public ObservableCollection<string> HostNames
        {
            get
            {
                return hostNames;
            }

            set
            {
                hostNames = value;
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



        public void SendText(string text)
        {
            try
            {
                if (this.Server != null)
                {
                    this.Server.SendText(text);
                }
                else
                {
                    this.AddInfoTextLine("Server Not Running");
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
                if (this.Server != null)
                {
                    this.Server.SendData(data);
                }
                else
                {
                    this.AddInfoTextLine("Server Not Running");
                }
            }
            catch (Exception ex)
            {
                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.Command = new RelayCommand(this.CommandReceived);
            this.InfoTextList = new ObservableCollection<string>();
            this.DataContext = this;

            cylictimer = new Timer(new TimerCallback(TimerProc), autoEvent, 0, 0);

            //libSharedProject.ProtolV1Commands.TestCmd c = new libSharedProject.ProtolV1Commands.TestCmd();
            //c.Title += "sws";

            var h = NetworkInformation.GetHostNames().Where(x => x.IPInformation != null && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6));

            foreach (var hn in h)
            {
                this.HostNames.Add(hn.DisplayName);
                this.Host = hn.DisplayName;
            }

            this.Server = new libCore.Async_TCP_StreamSocketServer();
            this.Server.NotifyTextEvent += Server_NotifyTextEvent;
            this.Server.NotifyexceptionEvent += Server_NotifyexceptionEvent;
            this.Server.NotifyMessageReceivedEvent += Server_NotifyMessageReceivedEvent;


            //for (int i = 0; i < 20; i++)
            //{
            //    this.AddInfoTextLine(i.ToString());
            //}           

        }

        private Windows.UI.Color logBackground;

        public Windows.UI.Color LogBackground
        {
            get { return logBackground; }
            set
            {
                logBackground = value;
                this.OnPropertyChanged();
            }
        }


        static ProtocolV1Base ProtocolV1BaseObj = new ProtocolV1Base();
        static string ProtocolV1Marker = nameof(ProtocolV1BaseObj.MyType);

        //App_IO_Demo.IoDemoBoard ioDemoBoard = new App_IO_Demo.IoDemoBoard();


        libCore.IOevalBoard.IoDemoBoard ioDemoBoard = null; // @todo hier wieder einbauen, sobald passt. new libCore.IOevalBoard.IoDemoBoard();
        libCore.IOevalBoard.RGBstripe LEDstripe = new libCore.IOevalBoard.RGBstripe();

        private void Server_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            try
            {
                var obj = libSharedProject.ProtolV1Commands.ProtocolV1Base.ConvertJsonStingToObj(System.Text.Encoding.UTF8.GetString(data));

                if (obj != null)
                {
                    if (obj.GetType().Equals(typeof(libSharedProject.ProtolV1Commands.IoDemoGetRequest)))
                    {
                        switch (((libSharedProject.ProtolV1Commands.IoDemoGetRequest)obj).Key)
                        {
                            case IoDemoGetRequest.CmdValue.Adc:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetAdc()));

                                break;
                            case IoDemoGetRequest.CmdValue.Dac:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetDac()));

                                break;
                            case IoDemoGetRequest.CmdValue.Gpio:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetGpio()));

                                break;
                            case IoDemoGetRequest.CmdValue.Powerstate:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetPowerState()));

                                break;
                            case IoDemoGetRequest.CmdValue.Rgb:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetRgb()));

                                break;
                            case IoDemoGetRequest.CmdValue.State:

                                this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(ioDemoBoard.GetState()));

                                break;
                            default:
                                break;
                        }
                    }
                    else if (obj.GetType().Equals(typeof(IoDemoDac)))
                    {
                        ioDemoBoard.SetDac((IoDemoDac)obj);
                    }
                    else if (obj.GetType().Equals(typeof(IoDemoGpio)))
                    {
                        ioDemoBoard.SetGpio((IoDemoGpio)obj);
                    }
                    else if (obj.GetType().Equals(typeof(IoDemoPowerState)))
                    {
                        ioDemoBoard.SetPowerState((IoDemoPowerState)obj);
                    }
                    else if (obj.GetType().Equals(typeof(IoDemoState)))
                    {
                        ioDemoBoard.SetState((IoDemoState)obj);
                    }
                    else if (obj.GetType().Equals(typeof(IoDemoRgb)))
                    {
                        ioDemoBoard.SetRgb((IoDemoRgb)obj);
                    }
                    else if (obj.GetType().Equals(typeof(RGBstripeColor)))
                    {
                        LEDstripe.SetSingleColor((RGBstripeColor)obj);
                    }
                }
                else
                {
                    this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data) + " Data:" + Converters.ConvertByteArrayToHexString(data, " "));
                }
            }
            catch (Exception ex)
            {

                try
                {
                    this.SendText(Newtonsoft.Json.JsonConvert.SerializeObject(new IoDemoException() { IoException = ex }));
                }
                catch (Exception eex)
                {
                    AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), eex)));
                    //ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
                    //throw;
                }


                AddInfoTextLine(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
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

        private void TimerProc(object state)
        {
            //// The state object is the Timer object.
            //Timer t = (Timer)state;
            //t.Dispose();
            ////Console.WriteLine("The timer callback executes.");

            string text = DateTime.Now.ToString();

            AddInfoTextLine(text);

            if (this.Server != null)
            {
                this.Server.SendText(text);
            }
        }

        private void Server_NotifyTextEvent(object sender, string text)
        {
            AddInfoTextLine(string.Format("Info from Server: {0}", text));
        }

        private void Server_NotifyexceptionEvent(object sender, Exception ex)
        {
            AddInfoTextLine(string.Format("Exception from Server: {0}", libShared.ExceptionHandling.GetExceptionText(ex)));
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
                    case "Start TCP Server":

                        this.Server.HostNameOrIp = this.Host;
                        this.Server.Port = this.Port;
                        this.AddInfoTextLine(string.Format("Server Start Beginn -> Host: {0}, Port: {1}", this.Server.HostNameOrIp, this.Server.Port));
                        await this.Server.Start();
                        this.AddInfoTextLine("Server Start: Ende");
                        break;

                    case "Stop TCP Server":
                        this.AddInfoTextLine("Server Stop: Beginn");
                        await this.Server.Stop();
                        this.AddInfoTextLine("Server Stop: Ende");
                        break;

                    case "Send TextBlock":
                        this.SendText(this.ValueSendText);
                        break;

                    case "Send Data":
                        this.SendData(this.ValueSendData.ToArray());
                        break;

                    case "Start Timer":
                        StartTimer();
                        break;

                    case "Stop Timer":
                        StopTimer();
                        break;

                    default:
                        this.AddInfoTextLine("Unknown CommandReceived: " + param);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.AddInfoTextLine("Exception: " + libShared.ExceptionHandling.GetExceptionText(ex));
            }
        }

        public void StartTimer()
        {
            try
            {
                AddInfoTextLine("Start Timer");
                cylictimer.Change(100, 100);
                //autoEvent.Reset();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void StopTimer()
        {
            try
            {
                AddInfoTextLine("Stop Timer");
                cylictimer.Change(0, 0);
                //autoEvent.Set();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }


        /// <summary>
        /// Fühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
        }

        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public async void AddInfoTextLine(object sender, string line)
        {
            try
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (this.InfoText.Length > 1000)
                    {
                        this.InfoText = line + Environment.NewLine;
                    }
                    else
                    {
                        this.InfoText += line + Environment.NewLine;
                    }

                    Scoll1.ChangeView(null, Scoll1.ScrollableHeight, null);

                    //this.InfoTextList.Add(line);                    
                });
            }
            catch (Exception ex)
            {
                ShowMessageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        private async void ShowMessageBox(string msg)
        {
            var msgDlg = new Windows.UI.Popups.MessageDialog(msg);
            msgDlg.DefaultCommandIndex = 1;
            await msgDlg.ShowAsync();
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        #region Porperty Changed

        /// <summary>
        /// PropertyChanged EventHandler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyName">Name der Property welche sich geändert hat.</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.CommandReceived("Start TCP Server");
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //var s = sender as ScrollViewer;
            //s.ChangeView(null, s.ScrollableHeight, null);
        }
    }
}
