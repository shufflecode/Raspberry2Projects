using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;

namespace WebServer
{
    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private readonly int _port;
        private readonly StreamSocketListener _listener;
        private HttpResponseMessage _response;

        public HttpServer(int serverPort)
        {
            _listener = new StreamSocketListener();
            _port = serverPort;
            _listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public async void Start()
        {
            await _listener.BindServiceNameAsync(_port.ToString());
        }

        public void Dispose()
        {
            _listener.Dispose();
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
                
                _response = RouteManager.CurrentRouteManager.InvokeMethod(request.ToString());

                using (IOutputStream output = socket.OutputStream)
                {
                    await WriteResponseAsync(_response, output);
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

    /// <summary>
    /// Verwaltet die ApiRouten des Servers
    /// </summary>
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
            Routes = new List<Route>();
            Controllers = new List<BaseClasses.ApiController>();
        }

        /// <summary>
        /// Alle verfügbaren Routen der Registrierten Controller
        /// </summary>
        public List<Route> Routes;

        /// <summary>
        /// Alle Registrierten Controller der Api
        /// </summary>
        public List<BaseClasses.ApiController> Controllers;

        /// <summary>
        /// Ermittelt aus dem rohen Request die aufzurufende Methode und 
        /// ruft diese mit den nnthaltenen Informationen und Paramtern auf
        /// </summary>
        /// <param name="reqstring">der ungefilterte HTTPRequest</param>
        public HttpResponseMessage InvokeMethod(string reqstring)
        {
            HttpResponseMessage retval;
            var request = new Request(reqstring);
            
            Route methodToInvoke = FindRoute(request.Path);
            if (methodToInvoke == null)
            {
                var response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("Keine Route gefunden :/");
                return response;
            }

            var param = methodToInvoke.Params.FirstOrDefault();
            var type = param?.ParameterType;
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                JsonConvert.PopulateObject(request.Content, instance);
                retval = methodToInvoke.Method.Invoke(methodToInvoke.Controller, new [] { instance }) as HttpResponseMessage;
            }
            else
            {
                retval = methodToInvoke.Method.Invoke(methodToInvoke.Controller, null) as HttpResponseMessage;
            }

            return retval;
        }

        /// <summary>
        /// Findet die Route zum aktuellen Request
        /// </summary>
        /// <param name="reqstring">HTTP Request</param>
        /// <returns>Die Route mit Methode zum Ausführen</returns>
        private Route FindRoute(string reqstring)
        {
            string strRegex = @"/\w+/*/.+";
            Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            
            foreach (Match myMatch in myRegex.Matches(reqstring))
            {
                if (myMatch.Success)
                {
                    var targetRoute = Routes.FirstOrDefault(
                        route => string.Equals(route.Url, myMatch.Value.Trim(), StringComparison.CurrentCultureIgnoreCase));
                    return targetRoute;
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
                var controllerType = apiController.GetType();
                var methodsWithRoutes = controllerType.GetMethods().Where(
                    m => m.GetCustomAttributes(typeof(Route)).Any() ).ToArray();
                foreach (var memberInfo in methodsWithRoutes)
                {
                    var  route = memberInfo.GetCustomAttributes(typeof (Route)).FirstOrDefault() as Route;
                    if (route == null) continue;
                    route.Method = memberInfo;
                    route.Controller = apiController;
                    route.Params = memberInfo.GetParameters();
                    Routes.Add(route);
                }
            }
        }
    }

    /// <summary>
    /// Enthält Informationen zu einer ApiRoute
    /// </summary>
    internal class Route : Attribute
    {
        public Type MethodType;
        public string Url{get; set;}
        public MethodInfo Method { get; set; }
        public BaseClasses.ApiController Controller { get; internal set; }
        public ParameterInfo[] Params { get; set; }

        public Route(string route,Type type)
        {
            Url = route;
            MethodType = type;
        }

        public override string ToString()
        {
            return Url;
        }

        public enum Type
        {
            Get =1,
            Post =2
        }
    }

    /// <summary>
    /// Hilfsklasse zum Parsen eines HTTPRequests
    /// </summary>
    class Request
    {
        private readonly string _rawstring;
        public RequestType Type;
        public int ContentLenght { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }

        public Request(string rawrequest)
        {
            _rawstring = rawrequest;
            Type = GetRequestType(_rawstring);
            Path = GetRequestPath(_rawstring);
            ContentLenght = GetContentLenght(_rawstring);
            Content = GetContent(_rawstring);
        }
        
        private static string GetContent(string request)
        {
            const string strRegex = @"{((?>[^{}]+|{(?<c>)|}(?<-c>))*(?(c)(?!))).";
            var myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            foreach (Match myMatch in myRegex.Matches(request))
            {
                if (myMatch.Success)
                {
                    return myMatch.Value;
                }
            }
            return string.Empty;
        }

        private static int GetContentLenght(string request)
        {
            int lenght = 0;
            string strRegex = @"Content-Length:.*";
            Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            foreach (Match myMatch in myRegex.Matches(request))
            {
                if (myMatch.Success)
                {
                    strRegex = @"[0-9]";
                    Regex numbers = new Regex(strRegex, RegexOptions.IgnoreCase);
                    foreach (Match numbermatch in numbers.Matches(myMatch.Value))
                    {
                        if (numbermatch.Success)
                        {
                            int.TryParse(numbermatch.Value, out lenght);
                        }
                    }
                }
            }
            return lenght;
        }

        private static RequestType GetRequestType(string reqstring)
        {
            RequestType type;
            var words = reqstring.Split(' ');
            Enum.TryParse(words[0], out type);
            return type;
        }

        private static string GetRequestPath(string reqstring)
        {
            var words = reqstring.Split(' ');
            return words[1];
        }
        public enum RequestType
        {
            Get,
            Post
        }
    }
}
