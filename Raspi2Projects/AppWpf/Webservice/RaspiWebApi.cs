using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using Exception = System.Exception;
using String = System.String;

namespace SanaScanV2.Webservice
{
   

    class RaspiApi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static ApiCustomer GetCustomer(int customerID)
        {
            var customer = new ApiCustomer();
            customer.CustomerID = customerID;
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.customer = customer;

            var returnedvalue = CallWebApi(Methodname.GetCustomer, apiTransferObject);

            JsonConvert.PopulateObject(returnedvalue.Item1.ToString(), customer);
            return customer;

        }

        /// <summary>
        /// Holt eine Liste der berechtigten CMS USer
        /// </summary>
        /// <returns></returns>
        public static List<ApiScanUser> GetScannerUsers()
        {
            var scannerUsers = new List<ApiScanUser>();
            var returnedvalue = CallWebApi(Methodname.Scanusers, new ApiTransferObject());

            foreach (var val in returnedvalue.Item1)
            {
                var user = new ApiScanUser();
                JsonConvert.PopulateObject(val.ToString(), user);
                scannerUsers.Add(user);
            }
            return scannerUsers;
        }

        /// <summary>
        /// Basis Order Informationen
        /// </summary>
        /// <param name="orderID">die ID der Order</param>
        /// <returns>ApiORder</returns>
        public static ApiOrder Getorder(int orderID)
        {
            var apiTransferObject = new ApiTransferObject();
            var order = new ApiOrder();
            order.ID = orderID;
            apiTransferObject.order = order;

            var val = CallWebApi(Methodname.Order, apiTransferObject);
            JsonConvert.PopulateObject(val.Item1.ToString(), order);
            //ToDo: Prüfung .. Wenn Auftrag in BEarbeitung ist, gibt es hier keine Values :/
            return order;
        }

        /// <summary>
        /// Erstellt einen Leerauftrag
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>ApiObject mit Auftragsnummer</returns>
        public static ApiOrder CreateEmptyOrder(int customerID)
        {
            var apiTransferObject = new ApiTransferObject();
            var customer = new ApiCustomer();
            customer.CustomerID = customerID;
            apiTransferObject.customer = customer;
            var val = CallWebApi(Methodname.CreateOrder, apiTransferObject);
            var order = new ApiOrder();
            JsonConvert.PopulateObject(val.Item1.ToString(), order);
            return order;
        }

        /// <summary>
        /// holt die Artiklel eines Auftrags
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns>Liste der Artikel eines Auftrags</returns>
        public static Tuple<List<ApiArticle>, ApiMessage> GetArticles(int OrderID)
        {
            var articles = new List<ApiArticle>();

            var apiTransferObject = new ApiTransferObject();
            var order = new ApiOrder();
            order.ID = OrderID;
            apiTransferObject.order = order;
            var values = CallWebApi(Methodname.GetArticles, apiTransferObject);

            foreach (var val in values.Item1)
            {
                var apiArticle = new ApiArticle();
                JsonConvert.PopulateObject(val.ToString(), apiArticle);
                articles.Add(apiArticle);
            }

            var sortedList = new List<ApiArticle>();

            //zunächst Alle nicht bundle Artikel 
            foreach (ApiArticle apiArticle in articles)
            {
                if (apiArticle.MainOrdersArticleID > 0)
                    continue;
                sortedList.Add(apiArticle);
            }

            //Nur noch subartikel in der Liste behalten 
            articles = articles.Where(article => article.MainOrdersArticleID > 0).ToList();


            foreach (var article in articles)
            {
                //Wenn BundleArtikel in liste vorhanden
                ApiArticle mainarticle = sortedList.First(o => o.OrdersArticleID == article.MainOrdersArticleID);

                if (mainarticle != null)
                {
                    if (sortedList.Any(o => o.MainOrdersArticleID == mainarticle.OrdersArticleID))
                        continue;
                    var subarticles = articles.FindAll(o => o.MainOrdersArticleID == mainarticle.OrdersArticleID).ToList();
                    int index = sortedList.IndexOf(mainarticle) + 1;

                    foreach (var subarticle in subarticles)
                    {
                        sortedList.Insert(index++, subarticle);
                    }
                }
            }

            return new Tuple<List<ApiArticle>, ApiMessage>(sortedList, values.Item2);
        }

        /// <summary>
        /// aufruf WS zum aktualisieren eines Artikels
        /// </summary>
        /// <param name="order"></param>
        /// <param name="article"></param>
        public static ApiMessage UpdateOrderArticle(ApiOrder order, ApiArticle article)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.order = order;
            apiTransferObject.Article = article;
            var values = CallWebApi(Methodname.UpdateOrderArticleCount, apiTransferObject);
            return values.Item2;
        }

        /// <summary>
        /// Entfernt einen Artikel vom Auftrag 
        /// </summary>
        /// <param name="apiOrder"></param>
        /// <param name="article"></param>
        public static ApiMessage DeleteOrderArticle(ApiOrder apiOrder, ApiArticle article)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.Article = article;
            var values = CallWebApi(Methodname.DeleteOrderArticle, apiTransferObject);
            return values.Item2;
        }

        /// <summary>
        /// Aufruf WS zum Blockieren eines Auftrags mit dem "in BEarbeitung Flag"
        /// </summary>
        /// <param name="_orderID">Auftragsnummer</param>
        public static ApiMessage BlockOrder(ApiOrder order, ApiScanUser user)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.order = order;
            apiTransferObject.user = user;
            var values = CallWebApi(Methodname.Block, apiTransferObject);
            return values.Item2;
        }

        /// <summary>
        /// Aufruf WS zum Freigeben eines Auftrags in Berbeitung
        /// </summary>
        /// <param name="order">Der freizugebende Auftrag</param>
        /// <param name="user">Der bearbeitende Nutzer</param>
        public static ApiMessage ReleaseOrder(ApiOrder order, ApiScanUser user)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.order = order;
            apiTransferObject.user = user;
            var values = CallWebApi(Methodname.Release, apiTransferObject);
            return values.Item2;
        }

        /// <summary>
        /// Storniert einen Auftrag
        /// </summary>
        /// <param name="apiOrder">der zu stornierende Auftrag</param>
        public static ApiMessage StornoOrder(ApiOrder apiOrder, ApiScanUser user)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.order = apiOrder;
            apiTransferObject.user = user;
            var values = CallWebApi(Methodname.Storno, apiTransferObject);
            return values.Item2;
        }

        /// <summary>
        /// Druckt einen DHL Kleber für diesen Auftrag
        /// </summary>
        /// <param name="id"></param>
        public static ApiMessage PrintDHLOrder(ApiOrder apiorder)
        {
            var apiTransferObject = new ApiTransferObject();
            apiTransferObject.order = apiorder;
            var values = CallWebApi(Methodname.DHLPrint, apiTransferObject);
            return values.Item2;
        }

        #region WebApi

        /// <summary>
        /// Eigentlicher WebserviceAufruf mittels Transferobjekt und unterschiedlichen API 
        /// Methodenaufrufen
        /// </summary>
        /// <param name="methodName">Methodname der aufgerufen werden soll</param>
        /// <param name="apiTransferObject">Das Hilfsobjekt das die zu Übertragenen Daten enthält</param>
        /// <returns>Json Objekt</returns>
        private static Tuple<JsonValue, ApiMessage> CallWebApi(Methodname methodName, ApiTransferObject apiTransferObject)
        {
            string sMethodeName;

            var local = "http://api.sana-essence.ca.loc/sanascan/";
            var staging = "http://api-staging.sana-essence.de/sanascan/";
            var devPC = "http://192.168.33.79:50582/sanascan/";

            switch (methodName)
            {
                case Methodname.Scanusers:
                    sMethodeName = "Scanneruser";

                    break;
                case Methodname.Article:
                    sMethodeName = "article";
                    break;
                case Methodname.Order:
                    sMethodeName = "order/" + apiTransferObject.order.ID;
                    break;
                case Methodname.GetWork:
                    sMethodeName = "order/getwork";
                    break;
                case Methodname.GetArticles:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/articles";
                    break;
                case Methodname.CreateOrder:
                    sMethodeName = "customer/" + apiTransferObject.customer.CustomerID + "/CreateOrder/" + GlobalReferences.LoggedInUser.UserID;
                    break;
                case Methodname.GetCustomer:
                    sMethodeName = "customer/" + apiTransferObject.customer.CustomerID;
                    break;
                case Methodname.OrderArticle:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/Article/" + apiTransferObject.Article.ID + "/" + apiTransferObject.Article.count;
                    break;
                case Methodname.Release:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/Release/" + apiTransferObject.user.UserID;
                    break;
                case Methodname.Block:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/Block/" + apiTransferObject.user.UserID;
                    break;
                case Methodname.Storno:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/Storno/" + apiTransferObject.user.UserID;
                    break;
                case Methodname.UpdateOrderArticleCount:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/article/";
                    break;
                case Methodname.DeleteOrderArticle:
                    sMethodeName = "order/" + apiTransferObject.Article.OrdersArticleID;
                    break;
                case Methodname.DHLPrint:
                    sMethodeName = "order/" + apiTransferObject.order.ID + "/PrintDHL";
                    break;
                default:
                    throw new Exception("CallWebApi Fehler, Methodenname unbekannt");
            }


            try
            {
                var Username = String.Empty;
                if (methodName != Methodname.Scanusers)
                {
                    Username = GlobalReferences.LoggedInUser.Username;
                }
                var credentials = new NetworkCredential(Username, "gnatzkartoffel");
                var request = (HttpWebRequest)HttpWebRequest.Create(new Uri(staging + sMethodeName));


                if (methodName != Methodname.Scanusers)
                {
                    request.Credentials = credentials;
                }
                request.ContentType = "application/json";

                switch (methodName)
                {
                    case Methodname.Scanusers:
                    case Methodname.Article:
                    case Methodname.Order:
                    case Methodname.GetWork:
                    case Methodname.GetArticles:
                    case Methodname.GetCustomer:
                    case Methodname.OrderArticle:
                        request.Method = "GET";
                        break;
                    case Methodname.DeleteOrderArticle:
                        request.Method = "DELETE";
                        break;

                    case Methodname.CreateOrder:
                    case Methodname.Release:
                    case Methodname.Block:
                    case Methodname.Storno:
                    case Methodname.DHLPrint:
                        request.Method = "POST";
                        request.ContentLength = 0;
                        break;
                    case Methodname.UpdateOrderArticleCount:
                        request.Method = "POST";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            string json = JsonConvert.SerializeObject(apiTransferObject.Article);
                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                            Console.Write(json);
                        }
                        break;
                }

                var response = (HttpWebResponse)request.GetResponse();

                TextReader tr = new StreamReader(response.GetResponseStream());
                var content = tr.ReadToEnd();

                JsonValue returnedJson = new JsonObject();
                ApiMessage returnedMessage = new ApiMessage();

                switch (response.Headers["Content-Type"])
                {
                    //ApiMessage 
                    case ContentType.ApiMessage:
                        returnedMessage.Parse(content);
                        break;
                    //Json Objects
                    case ContentType.json:
                        returnedJson = JsonValue.Parse(JsonConvert.DeserializeObject(content).ToString());
                        break;
                }

                var returnvalue = new Tuple<JsonValue, ApiMessage>(returnedJson, returnedMessage);

                return returnvalue;
            }
            catch (Exception ex)
            {
                ApiMessage message = new ApiMessage();
                message.message = ex.Message;
                message.Type = ApiMessage.MessageType.ErrorMessage;
                string sErrorMsg = "Exception: " + ex.Message + Environment.NewLine;
                sErrorMsg += "Methode: " + sMethodeName + Environment.NewLine;
                sErrorMsg += "Stacktrace: " + ex.StackTrace;
                return new Tuple<JsonValue, ApiMessage>(new JsonObject(), message);
            }
        }

        /// <summary>
        /// Namen der Methoden die der Webservice bereitstellt
        /// </summary>
        public enum Methodname
        {
            Scanusers = 1,
            Article = 2,
            Order = 3,
            GetWork = 4,
            GetArticles = 5,
            CreateOrder = 6,
            OrderArticle = 7,
            GetCustomer = 8,
            Block = 9,
            Release = 10,
            Storno = 11,
            UpdateOrderArticleCount = 12,
            DHLPrint = 13,

            DeleteOrderArticle
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