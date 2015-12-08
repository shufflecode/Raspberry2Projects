

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace AppUwaInterfacesDemoV1
{
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
    using libCore;
    using libShared;

    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        public static MainPage Current;

        private ObservableCollection<Scenario> pageCollection = new ObservableCollection<Scenario>();

        public ObservableCollection<Scenario> PageCollection
        {
            get { return pageCollection; }
        }

        private Page displayPage;

        public Page DisplayPage
        {
            get
            {
                return displayPage;
            }
            set
            {
                if (displayPage == value)
                {
                    return;
                }

                this.displayPage = value;
                OnPropertyChanged();
            }
        }

        private int displayPageIndex;

        public int DisplayPageIndex
        {
            get { return displayPageIndex; }
            set
            {
                displayPageIndex = value;
                this.DisplayPage = PageCollection[displayPageIndex].PageObj;
                this.OnPropertyChanged();
            }
        }



        /// <summary>
        /// Command Objekt (MVVM): Empfängt Commands von der Oberfläche
        /// </summary>
        public System.Windows.Input.ICommand Command { get; protected set; }

        private ObservableCollection<string> infoboxCollection = new ObservableCollection<string>();

        private string statusText;

        Visibility visStatusBar = Visibility.Collapsed;

        //List<object> pages = new List<object>();

        string infoText = string.Empty;

        public string InfoText
        {
            get { return infoText; }
            set
            {
                infoText = value;
                this.OnPropertyChanged();
            }
        }

        Page selectedPage;
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();


        public string TextLines { get; set; }

        public MainPage()
        {
            this.InitializeComponent();


            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            this.Command = new RelayCommand(this.CommandReceived);

            this.DataContext = this;

            //this.PageCollection.Add(new Scenario() { Title = "TCP Server", PageObj = new Lib_UWP_Network.UcTcpServer() });
            //this.PageCollection.Add(new Scenario() { Title = "TCP Client", PageObj = new Lib_UWP_Network.UcTcpClient() });

            //var eins = new Async_TCP_StreamSocketServer();
            //var zwei = new libCore.UserControls.UcEthernet(eins);
            //var drei = new libCore.Pages.PageEthernet(zwei);

            //drei.Uc.CommandReceived("Start");

            //zwei.Title = "dfdf";

            this.PageCollection.Add(new Scenario() { Title = "TCP Server", PageObj = new libCore.Pages.PageEthernet(new Async_TCP_StreamSocketServer()) });
            this.PageCollection.Add(new Scenario() { Title = "TCP Client", PageObj = new libCore.Pages.PageEthernet(new Async_TCP_StreamSocketClient()) });
            this.PageCollection.Add(new Scenario() { Title = "UART", PageObj = new libCore.Pages.PageSerial() });

            this.InfoTextList = new ObservableCollection<string>();

            //this.AddInfoTextLine("Hallo Welt");
            this.AddInfoTextLine("Hallo Welt");

            this.DisplayPageIndex = 0;

        }

        /// <summary>
        /// View: String Collection in der Informationen Angezeigt werden können.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> InfoTextList { get; internal set; }

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
                    case "b1":
                        this.AddInfoTextLine("CommandReceived: " + param);
                        break;
                    default:
                        this.AddInfoTextLine("Unknown CommandReceived: " + param);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.AddInfoTextLine("Exception: " + ex.Message);
            }
        }

        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                this.OnPropertyChanged();

                if (string.IsNullOrEmpty(value))
                {
                    this.VisStatusBar = Visibility.Collapsed;
                }
                else
                {
                    this.VisStatusBar = Visibility.Visible;
                }
            }
        }

        public ObservableCollection<string> InfoboxCollection
        {
            get { return infoboxCollection; }
        }

        public Visibility VisStatusBar
        {
            get { return visStatusBar; }
            set
            {
                visStatusBar = value;
                this.OnPropertyChanged();
            }
        }



        //public List<object> Pages
        //{
        //    get
        //    {
        //        return pages;
        //    }

        //    set
        //    {
        //        pages = value;
        //    }
        //}

        //public Page SelectedPage
        //{
        //    get
        //    {
        //        return selectedPage;
        //    }

        //    set
        //    {
        //        selectedPage = value;
        //        this.OnPropertyChanged();
        //    }
        //}



        /// <summary>
        /// ühgt der Infotext Liste einen weiteren Eintrag hinzu
        /// </summary>      
        public void AddInfoTextLine(string line)
        {
            this.AddInfoTextLine(null, line);
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
                });
            }
            catch (Exception ex)
            {
                messageBox(ExceptionHandling.GetExceptionText(new System.Exception(string.Format("Exception In: {0}", CallerName()), ex)));
            }
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

        ///// <summary>
        ///// Called whenever the user changes selection in the scenarios list.  This method will navigate to the respective
        ///// sample scenario page.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //    // Navigate to the appropriate destination page, configuring the new page
        //    // by passing required information as a navigation parameter
        //    //var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;

        //    //// Clear the status block when navigating scenarios.
        //    //NotifyUser(String.Empty, NotifyType.StatusMessage);



        //    //ListBox scenarioListBox = sender as ListBox;
        //    //Type ty = scenarioListBox.SelectedItem.GetType();
        //    //ScenarioFrame.Navigate(ty);

        //    ListBox scenarioListBox = sender as ListBox;
        //    Scenario s = scenarioListBox.SelectedItem as Scenario;
        //    if (s != null)
        //    {

        //        ScenarioFrame.Navigate(s.ClassType);
        //        if (Window.Current.Bounds.Width < 640)
        //        {
        //            Splitter.IsPaneOpen = false;
        //        }
        //    }
        //}

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    // Populate the scenario list from the SampleConfiguration.cs file
        //    ScenarioControl.ItemsSource = scenarios;
        //    if (Window.Current.Bounds.Width < 640)
        //    {
        //        ScenarioControl.SelectedIndex = -1;
        //    }
        //    else
        //    {
        //        ScenarioControl.SelectedIndex = 0;
        //    }
        //}

        #region ScrollViewer

        /// <summary>
        /// Hilfs Varibale
        /// </summary>
        private bool autoScroll = true;

        ///// <summary>
        ///// Hilfs Methode für den ScrollViewr, damit dieser Automatisch an das Ende scrollt.
        ///// </summary>
        ///// <param name="sender">Sender objcect</param>
        ///// <param name="e">ScrollChanged EventArgs</param>
        //private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    try
        //    {
        //        //// User scroll event : set or unset autoscroll mode
        //        if (e.ExtentHeightChange == 0)
        //        {
        //            //// Content unchanged : user scroll event
        //            if (((ScrollViewer)sender).VerticalOffset == ((ScrollViewer)sender).ScrollableHeight)
        //            {
        //                //// Scroll bar is in bottom
        //                //// Set autoscroll mode
        //                this.autoScroll = true;
        //            }
        //            else
        //            {   //// Scroll bar isn't in bottom
        //                //// Unset autoscroll mode
        //                this.autoScroll = false;
        //            }
        //        }

        //        //// Content scroll event : autoscroll eventually
        //        if (this.autoScroll && e.ExtentHeightChange != 0)
        //        {
        //            //// Content changed and autoscroll mode set
        //            //// Autoscroll
        //            ((ScrollViewer)sender).ScrollToVerticalOffset(((ScrollViewer)sender).ExtentHeight);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Exception exx = new System.Exception(string.Format(ET_100.Language.ErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name), ex);
        //        MessageBox.Show(ET_100.Functions.ExceptionHandling.GetExceptionText(exx));
        //    }
        //}

        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //ScenarioControl.SelectedIndex = 0;
        }
    }

}
