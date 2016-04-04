﻿

using System.ComponentModel;

namespace libSharedProject.ProtolV1Commands
{
    public class ProtocolV1Base
    {

#if WINDOWS_UWP
#else
        [System.ComponentModel.Browsable(false)]
#endif
        public string MyType
        {
            get { return this.GetType().Name; }
        }

        public static object ConvertJsonStingToObj(string json)
        {
            var obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            if (obj == null) return null;

            Newtonsoft.Json.Linq.JToken value;

            if (obj.TryGetValue("MyType", out value) == false) return null;

            string mdName = value.ToString();

            if (mdName == nameof(TestCmd))
            {
               return (TestCmd)obj.ToObject(typeof(TestCmd));
            }
            else if (mdName == nameof(IoDemoRgb))
            {
                return (IoDemoRgb)obj.ToObject(typeof(IoDemoRgb));
            }
            else if (mdName == nameof(IoDemoAdc))
            {
                return (IoDemoAdc)obj.ToObject(typeof(IoDemoAdc));
            }
            else if (mdName == nameof(IoDemoDac))
            {
                return (IoDemoDac)obj.ToObject(typeof(IoDemoDac));
            }
            else if (mdName == nameof(IoDemoPowerState))
            {
                return (IoDemoPowerState)obj.ToObject(typeof(IoDemoPowerState));
            }
            else if (mdName == nameof(IoDemoRequest))
            {
                return (IoDemoRequest)obj.ToObject(typeof(IoDemoRequest));
            }
            else
            {
                return null;
            }
        }
    }
}
