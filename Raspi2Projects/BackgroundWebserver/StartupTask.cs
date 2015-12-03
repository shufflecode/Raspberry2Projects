using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.System.Threading;
using BackgroundWebserver.ApiController;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BackgroundWebserver
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.GetDeferral();
            
            try
            {
                RouteManager.CurrentRouteManager.Controllers.Add(new LedController());
                RouteManager.CurrentRouteManager.InitRoutes();
                IAsyncAction asyncAction = ThreadPool.RunAsync(workItem =>
                {
                    HttpServer server = new HttpServer(80);
                  
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler in Run: " + ex.Message);
            }
           
        }
    }

   
}
