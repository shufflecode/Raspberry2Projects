using System;
using System.Reflection;

namespace WebServer
{
    /// <summary>
    /// Attribute Class
    /// Contains information for the Route manager to find Api Routes 
    /// with an matching HTTP Request
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
}