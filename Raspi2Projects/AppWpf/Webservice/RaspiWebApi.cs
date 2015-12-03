using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using  System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using Exception = System.Exception;
using String = System.String;
using libShared.ApiModels;
namespace SanaScanV2.Webservice
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

     
        private static string CallWebApi(Methodname methodName)
        {
            string sMethodeName;

            var baseUrl = "http://192.168.0.11/";
           
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
                            //string json = JsonConvert.SerializeObject(apiTransferObject.Article);
                            //streamWriter.Write(json);
                            //streamWriter.Flush();
                            //streamWriter.Close();
                            //Console.Write(json);
                        }
                        break;
                }

                var response = (HttpWebResponse)request.GetResponse();

                TextReader tr = new StreamReader(response.GetResponseStream());
                var content = tr.ReadToEnd();

                string returnedJson = string.Empty;
                //ApiMessage returnedMessage = new ApiMessage();

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