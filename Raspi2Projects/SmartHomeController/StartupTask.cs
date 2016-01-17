using System;
using System.Diagnostics;
using System.IO;
using Windows.Web;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using IotWeb.Common;
using WebServer.ApiController;
using IotWeb.Server;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace WebServer
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;

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

                //Websocket server
                var srv = new SocketServer(8001);
                srv.ConnectionRequested += Requested;

                IAsyncAction asyncAction = ThreadPool.RunAsync(workItem =>
                {
                    server.Start();
                    srv.Start();
                    
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Run " + ex);
            }
        }

        private async void Requested(ISocketServer sender, string hostname, Stream input, Stream output)
        {
            try
            {
                var sr = new StreamReader(input);
                var result = sr.ReadLine();
                Debug.WriteLine("Request:" + sender + hostname + "Rsult :" + result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Requested:" + ex);
            }
            
        }
    }
}
