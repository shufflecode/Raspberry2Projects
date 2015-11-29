using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using WebServer.ApiController;
namespace WebServer
{
    internal class WebServer
    {
        private const uint BufferSize = 8192;

        public void Start()
        {
            var Routemanager = new RouteManager();
            var LEDController = new LedController();
            Routemanager.BaseRoutes.Add(new ControllerRoute(LEDController));
            StreamSocketListener listener = new StreamSocketListener();

           
            listener.BindServiceNameAsync("80");

            listener.ConnectionReceived += async (sender, args) =>
            {
                StringBuilder request = new StringBuilder();
                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    string reqstring = string.Empty;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        reqstring = request.ToString();
                        dataRead = buffer.Length;
                    }
                    Routemanager.InvokeMethod(reqstring);
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] bodyArray = Encoding.UTF8.GetBytes("<html><body>Hello, World!"+ "" +"</body></html>");
                        var bodyStream = new MemoryStream(bodyArray);

                        var header = "HTTP/1.1 200 OK\r\n" +
                                    $"Content-Length: {bodyStream.Length}\r\n" +
                                        "Connection: close\r\n\r\n";

                        byte[] headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray, 0, headerArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            };
        }
    }

    internal class RouteManager
    {
        private static RouteManager _instance;
        public static RouteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RouteManager();
                }
                return _instance;
            }
        }

        public RouteManager()
        {
            this.Routes = new List<Route>();
            this.BaseRoutes = new List<ControllerRoute>();
        }

        public List<Route> Routes;
        public List<ControllerRoute> BaseRoutes;

        public void InvokeMethod(string reqstring)
        {
            var controller = this.GetController(reqstring);
            var methodname = this.GetMethodName(controller, reqstring);
        }

        
        private BaseClasses.ApiController GetController(string reqstring)
        {
            string strRegex = @"/\w+/";
            Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);

            foreach (Match myMatch in myRegex.Matches(reqstring))
            {
                if (myMatch.Success)
                {
                    var routes = BaseRoutes.Where(o => o.URL == myMatch.Value).ToList();
                    
                    if(routes.Count( )>1)
                        throw new Exception("Zuviele Routen");
                    ControllerRoute route = routes.FirstOrDefault();
                    return route.Controller;
                }
            }
            return null;
        }

        private string GetMethodName(BaseClasses.ApiController controller,string request)
        {
            var ControllerType = controller.GetType();
            List<MethodInfo> methodInfos = ControllerType.GetMethods().ToList();
            string strRegex = @"/\w+/";
            Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            foreach (Match myMatch in myRegex.Matches(request))
            {
                if (myMatch.Success)
                {
                    var methods = methodInfos.Where(o => o.Name == myMatch.Value).ToList();
                    if (methods.Count() > 1 || !methods.Any())
                       continue;
                    MethodInfo methodInfo = methods.FirstOrDefault();
                    return methodInfo.Name;
                }
            }
            return  String.Empty;
        }
    }

    internal class Route : Attribute
    {
        public string URL{get; set;}

        public Route(string Route)
        {
            this.URL = Route;
        }
    }

    internal class ControllerRoute
    {
        public BaseClasses.ApiController Controller;
        public string URL { get; set; }

        public ControllerRoute(BaseClasses.ApiController controller)
        {
            this.Controller = controller;
            this.URL = controller.RouteBase;
        }
    }


}
