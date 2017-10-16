using System;
using System.IO;
using System.Net;
using libShared.ApiModels;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace AppWpf.Webservice
{
    class RaspiApi
    {
        public static LEDStatus GetLedStatus()
        {
            var status = new LEDStatus();
            var returnedvalue = CallWebApi(Methodname.GetLEDStatus);
            JsonConvert.PopulateObject(returnedvalue, status);
            return status;
        }
        #region WebApi

     
        private static string CallWebApi(Methodname methodName, object apiParams = null)
        {
            string sMethodeName;

            var baseUrl = "http://192.168.0.28/";
           
            switch (methodName)
            {
                case Methodname.GetLEDStatus:
                    sMethodeName = "LedController/Green";
                    break;
               
                default:
                    throw new Exception("CallWebApi Fehler, Methodenname unbekannt");
            }


            try
            {
               
                var request = (HttpWebRequest)HttpWebRequest.Create(new Uri(baseUrl + sMethodeName));
                
                //request.ContentType = "application/json";

                switch (methodName)
                {
                    case Methodname.GetLEDStatus:
                        request.Method = "GET";
                        break;
                    
                    case Methodname.WriteLEdStstus:
                        request.Method = "POST";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            var ledStatus = apiParams as LEDStatus;
                            var json = JsonConvert.SerializeObject(ledStatus);
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        break;
                }

                var response = (HttpWebResponse)request.GetResponse();

                TextReader tr = new StreamReader(response.GetResponseStream());
                var content = tr.ReadToEnd();

                string returnedJson = string.Empty;
                
                switch (response.Headers["Content-Type"])
                {
                    //Json Objects
                    case ContentType.json:
                        returnedJson = JsonConvert.DeserializeObject(content).ToString();
                        break;
                }

                return returnedJson;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Namen der Methoden die der Webservice bereitstellt
        /// </summary>
        public enum Methodname
        {
           GetLEDStatus,
            WriteLEdStstus
        }

        /// <summary>
        /// Hilfsklasse zur definition der Content Types
        /// </summary>
        public static class ContentType
        {
            public const string ApiMessage = "text/plain; charset=utf-8";
            public const string json = "application/json; charset=utf-8";
        }
        #endregion



    }
}