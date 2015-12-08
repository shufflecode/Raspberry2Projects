using libShared;
using libShared.Interfaces;
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

namespace libDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für UcEthernet.xaml
    /// </summary>
    public partial class UcEthernet : UserControl, INotifyPropertyChanged
    {
        IEthernetSync ethernet = null;

        private string valueSendText;

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
            get
            {
                return valueSendData;
            }

            set
            {
                valueSendData = value;
                this.OnPropertyChanged();
            }
        }

        public IEthernetSync Ethernet
        {
            get
            {
                return ethernet;
            }

            set
            {
                ethernet = value;
                Ethernet.NotifyexceptionEvent += Client_NotifyexceptionEvent;
                Ethernet.NotifyTextEvent += Client_NotifyTextEvent;
                Ethernet.NotifyMessageReceivedEvent += Client_NotifyMessageReceivedEvent;
            }
        }

        public UcEthernet()
        {
            InitializeComponent();
            InfoTextList = new ObservableCollection<object>();
            this.Command = new RelayCommand(this.CommandReceived);
            this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            this.DataContext = this;
        }

        public UcEthernet(IEthernetSync _Ethernet)
            : this()
        {
            this.Ethernet = _Ethernet;            
        }

        public void Start()
        {
            try
            {
                this.AddInfoTextLine("Beginn Start");

                this.Ethernet.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        public void Stop()
        {
            try
            {
                this.AddInfoTextLine("Beginn Stop");

                this.Ethernet.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        public void Exit()
        {
            try
            {
                this.AddInfoTextLine("Beginn Exit");
                this.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        private void Client_NotifyMessageReceivedEvent(object sender, byte[] data)
        {
            this.AddInfoTextLine(string.Format("TcpClient Message Received: {0}", Converters.ConvertByteArrayToHexString(data, " ")));
        }

        private void Client_NotifyTextEvent(object sender, string text)
        {
            this.AddInfoTextLine(string.Format("TcpClient Info Received: {0}", text));
        }

        private void Client_NotifyexceptionEvent(object sender, Exception ex)
        {
            this.AddInfoTextLine(string.Format("TcpClient Exception Received: {0}", ExceptionHandling.GetExceptionText(ex)));
        }

        #region AutoScroll

        /// <summary>
        /// Hilfs Varibale
        /// </summary>
        private bool autoScroll = true;

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

        #endregion

        #region InfoTextList

        // Konstruktor: this.DispatcherObject = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        //<ScrollViewer DockPanel.Dock="Bottom" ScrollChanged= "ScrollViewer_ScrollChanged" ScrollViewer.HorizontalScrollBarVisibility= "Auto" ScrollViewer.VerticalScrollBarVisibility= "Auto" >
        //    < ListBox ItemsSource= "{Binding InfoTextList}" BorderThickness= "0" />
        //</ ScrollViewer >

        /// <summary>
        /// Multi Threading Hilfs Objekt.
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Dispatcher Hilf Objekt.
        /// Stellt Dienste zum Verwalten der Warteschlange von Arbeitsaufgaben für einen Thread bereit.
        /// </summary>
        public virtual System.Windows.Threading.Dispatcher DispatcherObject { get; protected set; }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<object> InfoTextList { get; internal set; }

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

        #endregion

        #region Command_Handling

        //// Konstukor: this.Command = new Lib_Port_SharedCode.RelayCommand(this.CommandReceived);

        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

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
                    case "Start":
                        this.Start();
                        break;

                    case "Stop":
                        this.Stop();
                        break;

                    case "Exit":
                        this.Exit();
                        break;

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
                MessageBox.Show(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", System.Reflection.MethodBase.GetCurrentMethod().Name), ex)));
            }
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wird manuell Aufgerufen wenn sich eine Property ändert, dammit alle Elemente die an diese Property gebunden sind (UI-Elemente) aktualisiert werden.
        /// </summary>
        /// <param name="propertyname">Name der Property welche sich geändert hat.</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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
