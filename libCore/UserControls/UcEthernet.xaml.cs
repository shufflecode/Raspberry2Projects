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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace libCore.UserControls
{
    using libShared;
    using libShared.Interfaces;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Networking.Connectivity;
    using Windows.UI.Core;
    using Windows.UI.Xaml.Controls;

    public sealed partial class UcEthernet : UserControl, INotifyPropertyChanged
    {
        private string title = "Async GUI";
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        IEthernetAsync ethernet = null;
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

        public IEthernetAsync Ethernet
        {
            get { return ethernet; }
            set
            {
                ethernet = value;
                ethernet.NotifyTextEvent += AddInfoTextLine;
                ethernet.NotifyexceptionEvent += Ethernet_NotifyexceptionEvent;
                ethernet.NotifyMessageReceivedEvent += Ethernet_NotifyMessageReceivedEvent;
            }
        }

        public UcEthernet()
        {
            this.InitializeComponent();
            

            this.InfoTextList = new ObservableCollection<object>();
            this.Command = new RelayCommand(this.CommandReceived);
            this.DataContext = this;
            var h = NetworkInformation.GetHostNames().Where(x => x.IPInformation != null && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6));

            //1   Some other type of network interface.
            //6   An Ethernet network interface.
            //9   A token ring network interface.
            //23  A PPP network interface.
            //24  A software loopback network interface.
            //37  An ATM network interface.
            //71  An IEEE 802.11 wireless network interface.
            //131 A tunnel type encapsulation network interface.
            //144 An IEEE 1394 (Firewire) high performance serial bus network interface.

            foreach (var hn in h)
            {
                this.HostNames.Add(hn.DisplayName);
                this.Host = hn.DisplayName;
            }            

            this.Title = "Default";
          
        }

        public UcEthernet(IEthernetAsync _ethernet)
            : this()
        {
            this.Ethernet = _ethernet;
            this.Title = this.Ethernet.GetType().Name;
            this.DataContext = this;
        }

        private void Ethernet_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            this.AddInfoTextLine("Text:" + System.Text.Encoding.UTF8.GetString(data));
            this.AddInfoTextLine("Data:" + Converters.ConvertByteArrayToHexString(data, " "));
        }

        private void Ethernet_NotifyexceptionEvent(object sender, Exception ex)
        {
            this.AddInfoTextLine(sender, ex.Message);
        }

        #region InfoTextList

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<object> InfoTextList { get; internal set; }

        string textBlockText1 = string.Empty;

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
        async public void AddInfoTextLine(object sender, string line)
        {
            try
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.InfoTextList.Add(line);
                    TextBlockText1 += string.Format("{0}\n", line);
                });
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        protected async void messageBox(string msg)
        {
            var msgDlg = new Windows.UI.Popups.MessageDialog(msg);
            msgDlg.DefaultCommandIndex = 1;
            await msgDlg.ShowAsync();
        }

        string CallerName([CallerMemberName]string caller = "")
        {
            return caller;
        }

        #endregion

        #region Command_Handling

        //// Konstukor: this.Command = new Lib_Port_SharedCode.RelayCommand(this.CommandReceived);

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        string port = "27200";
        string host = "localhost";

        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                this.OnPropertyChanged();
            }
        }

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                this.OnPropertyChanged();
            }
        }

        private string valueSendText = "Hallo Welt";

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

        public string TextBlockText1
        {
            get
            {
                return textBlockText1;
            }

            set
            {
                textBlockText1 = value;
                this.OnPropertyChanged();
            }
        }



        /// <summary>
        /// Empfängt Commands von der Oberfläche
        /// </summary>
        /// <param name="param">Empfangenes Command</param>
        async public void CommandReceived(object param)
        {
            try
            {
                switch (param as string)
                {
                    case "Start":
                        this.AddInfoTextLine("Server Start: Beginn");
                        this.Ethernet.HostNameOrIp = this.Host;
                        this.Ethernet.Port = this.Port;
                        await this.Ethernet.Start();
                        this.AddInfoTextLine("Server Start: Ende");
                        break;

                    case "Stop":
                        this.AddInfoTextLine("Server Stop: Beginn");
                        await this.Ethernet.Stop();
                        this.AddInfoTextLine("Server Stop: Ende");
                        break;

                    //case "Connect":

                    //    this.AddInfoTextLine("Connect Start");
                    //    this.client.Ip = this.Host;
                    //    this.client.Port = this.Port;
                    //    //await this.client.Connect(this.Host, this.Port);
                    //    await this.Client.Start();
                    //    this.AddInfoTextLine("Connect Ende");

                    //    break;

                    //case "Disconnect":

                    //    //this.AddInfoTextLine("Connect Start");
                    //    //await this.client.di(this.Host, this.Port);
                    //    //this.AddInfoTextLine("Connect Ende");

                    //    break;

                    //case "Read":

                    //    //string text = await this.client.Read();
                    //    //this.AddInfoTextLine(text);

                    //    this.client.ReadData();

                    //    break;

                    case "Send TextBlock":
                        this.SendText(this.ValueSendText);
                        break;

                    case "Send Data":
                        this.SendData(this.ValueSendData.ToArray());
                        break;

                    default:
                        this.AddInfoTextLine(string.Format("Command {0} not Implemented", param.ToString()));
                        break;
                }

            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        #endregion

        //async public Task<string> Read()
        //{
        //    string text = string.Empty;

        //    try
        //    {
        //        if (this.Client != null)
        //        {

        //            this.Client.ReadData();
        //            //text = await this.Client.ReadData();
        //            //this.AddInfoTextLine(text);
        //        }
        //        else
        //        {
        //            this.AddInfoTextLine("Client Not Running");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        messageBox(Lib_Port_SharedCode.ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
        //    }

        //    return text;
        //}

        public void SendText(string text)
        {
            try
            {
                if (this.Ethernet != null)
                {
                    this.Ethernet.SendText(text);
                }
                else
                {
                    this.AddInfoTextLine("TcpClient Not Running");
                }
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (this.Ethernet != null)
                {
                    this.Ethernet.SendData(data);
                }
                else
                {
                    this.AddInfoTextLine("TcpClient Not Running");
                }
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyname">Name der Property welche sich geändert hat.</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


    }
}
