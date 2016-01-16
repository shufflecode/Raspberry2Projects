using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WebServer
{
    /// <summary>
    /// Simple HTTP Server to serve a lightweight REST - API
    /// </summary>
    public sealed class HttpServer : IDisposable
    {
        #region private attributes
        private const uint BufferSize = 8192;
        private readonly int _port;
        private readonly StreamSocketListener _listener;
        private HttpResponseMessage _response;
        #endregion

        #region Constructors
        public HttpServer(int serverPort)
        {
            _listener = new StreamSocketListener();
            _port = serverPort;
            _listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }
        #endregion

        #region methods
        /// <summary>
        /// Starts the webserver
        /// </summary>
        public async void Start()
        {
            await _listener.BindServiceNameAsync(_port.ToString());
        }

        /// <summary>
        /// disposes the webserver
        /// </summary>
        public void Dispose()
        {
            _listener.Dispose();
        }

        /// <summary>
        /// Main Method to Process async requests
        /// </summary>
        /// <param name="socket"></param>
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
                
                _response = RouteManager.Current.InvokeMethod(request.ToString());

                using (IOutputStream output = socket.OutputStream)
                {
                    await WriteResponseAsync(_response, output);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in ProcessRequestAsync: " + ex);
            }
            
        }

        /// <summary>
        /// wites the webservers response to the outputstream
        /// </summary>
        /// <param name="message">teh MEssage to return to the client</param>
        /// <param name="os">Inputstream</param>
        /// <returns></returns>
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
                Debug.WriteLine("Error in WriteResponseAsync: " + ex);
            }
            
        }
        #endregion
    }
}
