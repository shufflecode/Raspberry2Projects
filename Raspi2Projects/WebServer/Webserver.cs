using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using WebServer.ApiController;
using WebServer.BaseClasses;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace WebServer
{
    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private int port = 80;
        private readonly StreamSocketListener listener;
        private HttpResponseMessage response;

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            port = serverPort;
            listener.BindServiceNameAsync(port.ToString());
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public void Dispose()
        {
            listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            try
            {
                StringBuilder request = new StringBuilder();
                using (IInputStream input = socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                }

                response = new HttpResponseMessage(HttpStatusCode.Found);
                response.Content = new StringContent("huhuh ?");
                //response = RouteManager.CurrentRouteManager.InvokeMethod(request.ToString());

                using (IOutputStream output = socket.OutputStream)
                {
                    await WriteResponseAsync(response, output);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler in ProcessRequestAsync: " + ex.Message);
            }
            
        }

        private async Task WriteResponseAsync(HttpResponseMessage message, IOutputStream os)
        {
            try
            {
                using (Stream resp = os.AsStreamForWrite())
                {
                    byte[] bodyArray = await message.Content.ReadAsByteArrayAsync();
                    MemoryStream stream = new MemoryStream(bodyArray);
                    message.Content.Headers.ContentLength = stream.Length;
                    string header = string.Format("HTTP/" + message.Version + " " + (int)message.StatusCode + " " + message.StatusCode + Environment.NewLine
                                                + "Content-Type: " + message.Content.Headers.ContentType + Environment.NewLine
                                                + "Content-Length: " + message.Content.Headers.ContentLength + Environment.NewLine
                                                + "Connection: close\r\n\r\n");
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    await stream.CopyToAsync(resp);
                    await resp.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler in WriteResponseAsync: " + ex.Message);
            }
            
        }
    }
    internal class RouteManager
    {
        private static RouteManager _instance;
        public static RouteManager CurrentRouteManager
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
        [Obsolete("Hier muss noch parameterparsing betrieben werden :(")]
        public HttpResponseMessage InvokeMethod(string reqstring)
        {
            //Todo: get object[] aus dem request und aufruf der Methode mit diesen parametern
            Route methodToInvoke = FindRoute(reqstring);
            var retval = methodToInvoke.Method.Invoke(methodToInvoke.Controller, new object[] {1});

            return retval as HttpResponseMessage;
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
