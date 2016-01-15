using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml;
using WebServer.ApiController;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace WebServer
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //Necessary to keep the App Running in BG
            this._deferral = taskInstance.GetDeferral();
            
            try
            {
                var server = new HttpServer(80);
                RouteManager.Current.Register(new LedController());
                RouteManager.Current.Register(new GPIOController());
                RouteManager.Current.InitRoutes();
                IAsyncAction asyncAction = ThreadPool.RunAsync(workItem =>
                {
                     server.Start();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Run " + ex);
            }
        }
    }
}
