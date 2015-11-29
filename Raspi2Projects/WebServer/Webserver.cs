using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
            Routemanager.Routes.Add(new Route(new LedController()));
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
                    var controller = Routemanager.GetRoute(reqstring);
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] bodyArray = Encoding.UTF8.GetBytes("<html><body>Hello, World!</body></html>");
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
        }

        public List<Route> Routes;

        public BaseClasses.ApiController GetRoute(string reqstring)
        {
            string strRegex = @"/\w+/";
            Regex myRegex = new Regex(strRegex, RegexOptions.None);

            foreach (Match myMatch in myRegex.Matches(reqstring))
            {
                if (myMatch.Success)
                {
                    var routes = Routes.Where(o => o.URL == myMatch.Value).ToList();
                    
                    if(routes.Count( )>1)
                        throw new Exception("Zuviele Routen");
                    Route route = routes.FirstOrDefault();
                    return route.Controller;
                }
            }
            return null;
        }
    }

    internal class Route
    {
        public BaseClasses.ApiController Controller { get; set; }

        public string URL{get; set;}

        public Route(BaseClasses.ApiController controller)
        {
            this.Controller = controller;
            this.URL = controller.RouteBase;
        }
    }

 
}
