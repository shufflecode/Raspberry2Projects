using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace WebServer
{
    /// <summary>
    /// Singleton
    /// Manages Api Routes for the Webserver
    /// </summary>
    internal class RouteManager
    {
        private static RouteManager _instance;
        public static RouteManager Current
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

        #region methods
        /// <summary>
        /// Contains all available ApiRoutes for the ApiControllers registered in this routemanager
        /// </summary>
        public List<Route> Routes;

        /// <summary>
        /// Contains all available ApiControllers
        /// </summary>
        public List<BaseClasses.ApiController> Controllers;

        /// <summary>
        /// Trys to find the Route to the responsible ApiController
        /// and invokes the Method with parameters if a match was found
        /// </summary>
        /// <param name="reqstring">raw HTTPRequest</param>
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
        /// Finds the rout for the current Request
        /// </summary>
        /// <param name="reqstring">raw HTTPRequest</param>
        /// <returns>matching Api Route</returns>
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
        /// Registers an ApiController in this Routemanager
        /// </summary>
        /// <param name="controller">ApiController to Register</param>
        public void Register(BaseClasses.ApiController controller)
        {
            this.Controllers.Add(controller);
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

        #endregion
    }
}