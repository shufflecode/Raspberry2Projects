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
using WebServer.BaseClasses;

namespace WebServer
{
    internal class WebServer
    {
        private const uint BufferSize = 8192;

        public void Start()
        {
            var Routemanager = new RouteManager();
            var LEDController = new LedController();
            Routemanager.Controllers.Add(LEDController);
            Routemanager.InitRoutes();

            StreamSocketListener listener = new StreamSocketListener();

           
            listener.BindServiceNameAsync("80");

            listener.ConnectionReceived += async (sender, args) =>
            {
                string retval = String.Empty;
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
                    retval =  Routemanager.InvokeMethod(reqstring).ToString();
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] bodyArray = Encoding.UTF8.GetBytes("<html><body>Hello, World!"+ retval + "</body></html>");
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
            this.Controllers = new List<BaseClasses.ApiController>();
        }

        public List<Route> Routes;
        public List<BaseClasses.ApiController> Controllers;

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="reqstring"></param>
        public object InvokeMethod(string reqstring)
        {
            Route methodToInvoke = FindRoute(reqstring);
            //YES!!!
            return methodToInvoke.Method.Invoke(methodToInvoke.Controller,new object[] {1});
        }

        /// <summary>
        /// Findet die Route zum aktuellen Request
        /// </summary>
        /// <param name="reqstring">HTTP Request</param>
        /// <returns>Die Route mit Methode zum Ausführen</returns>
        private Route FindRoute(string reqstring)
        {
            string strRegex = @"/\w+/*/.+ ";
            Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            
            foreach (Match myMatch in myRegex.Matches(reqstring))
            {
                if (myMatch.Success)
                {
                    Route TargetRoute = Routes.FirstOrDefault(
                        route => String.Equals(route.URL, myMatch.Value.Trim(), StringComparison.CurrentCultureIgnoreCase));
                    return TargetRoute;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Initialisiert die Routen für alle verfügbaren Controller
        /// </summary>
        internal void InitRoutes()
        {
            foreach (BaseClasses.ApiController apiController in Controllers)
            {
                var ControllerType = apiController.GetType();
                MethodInfo[] methodsWithRoutes = ControllerType.GetMethods().Where(
                    m => m.GetCustomAttributes(typeof(Route)).Any() ).ToArray();
                foreach (var memberInfo in methodsWithRoutes)
                {
                    var  route = memberInfo.GetCustomAttributes(typeof (Route)).FirstOrDefault() as Route;
                    route.Method = memberInfo;
                    route.Controller = apiController;
                    route.Params = memberInfo.GetParameters();
                    Routes.Add(route);
                }
            }
        }
    }

    internal class Route : Attribute
    {
        public string URL{get; set;}
        public MethodInfo Method { get; set; }
        public BaseClasses.ApiController Controller { get; internal set; }
        public ParameterInfo[] Params { get; set; }

        public Route(string Route)
        {
            this.URL = Route;
        }

        public override string ToString()
        {
            return URL;
        }
    }
}
