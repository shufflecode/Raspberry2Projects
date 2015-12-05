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
            ///Wird das ding nicht als Attribut gehalten kommt der GC vorbei und die APP Stirbt :/ 
            this._deferral = taskInstance.GetDeferral();
            
            try
            {
                HttpServer server = new HttpServer(80);
                RouteManager.CurrentRouteManager.Controllers.Add(new LedController());
                RouteManager.CurrentRouteManager.Controllers.Add(new GPIOController());
                RouteManager.CurrentRouteManager.InitRoutes();
                IAsyncAction asyncAction = ThreadPool.RunAsync(workItem =>
                {
                     server.Start();
                  
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler in Run: " + ex.Message);
            }
           
        }
    }

   
}
