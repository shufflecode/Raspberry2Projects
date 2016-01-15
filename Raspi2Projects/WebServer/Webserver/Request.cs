using System;
using System.Text.RegularExpressions;

namespace WebServer
{
    /// <summary>
    /// Helper class for representing / parsing HTTP requests
    /// </summary>
    class Request
    {
        private readonly string _rawstring;
        public RequestType Type;
        public int ContentLenght { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public int RequestParameter { get; set; }

        public Request(string rawrequest)
        {
            _rawstring = rawrequest;
            Type = GetRequestType(_rawstring);
            Path = GetRequestPath(_rawstring);
            ContentLenght = GetContentLenght(_rawstring);
            Content = GetContent(_rawstring);
            RequestParameter = GetParameter(Path);
        }

        /// <summary>
        /// Trys to find Integer Parameter in Request Path 
        /// and gets it out. 
        /// </summary>
        /// <param name="rawstring">the raw Route</param>
        /// <returns>integer Parameter</returns>
        private int GetParameter(string rawstring)
        {
            var index = rawstring.LastIndexOf("/");
            var param = rawstring.Substring(index);
            param = param.Replace("/", string.Empty);
            int Parameter = 0;
            int.TryParse(param, out Parameter);
            if (RequestParameter > 0)
            {
                Path = Path.Replace(rawstring.Substring(index), string.Empty);
            }
            return Parameter;
        }

        /// <summary>
        /// Gets the Content out of the raw HTTP Request
        /// </summary>
        /// <param name="request">raw HTTP Request </param>
        /// <returns>HTTP Content of the Reqest</returns>
        private string GetContent(string request)
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

        /// <summary>
        /// Parses the Content Lenght out of the raw request
        /// </summary>
        /// <param name="request">raw HTTP Request </param>
        /// <returns>int Content Lenght</returns>
        private int GetContentLenght(string request)
        {
            int lenght = 0;
            string strRegex = @"Content-Length:.*";
            var myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);
            foreach (Match myMatch in myRegex.Matches(request))
            {
                if (!myMatch.Success) continue;
                strRegex = @"[0-9]";
                var numbers = new Regex(strRegex, RegexOptions.IgnoreCase);
                foreach (Match numbermatch in numbers.Matches(myMatch.Value))
                {
                    if (numbermatch.Success)
                    {
                        int.TryParse(numbermatch.Value, out lenght);
                    }
                }
            }
            return lenght;
        }

        /// <summary>
        /// Gets the HTTP reqest type 
        /// </summary>
        /// <param name="reqstring">raw HTTP Request</param>
        /// <returns>Request Type</returns>
        private RequestType GetRequestType(string reqstring)
        {
            RequestType type;
            var words = reqstring.Split(' ');
            Enum.TryParse(words[0], out type);
            return type;
        }

        /// <summary>
        /// Parses the route out of the raw HTTP Reuest 
        /// </summary>
        /// <param name="reqstring"></param>
        /// <returns>strin representation of the route to match later in APIController</returns>
        private string GetRequestPath(string reqstring)
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