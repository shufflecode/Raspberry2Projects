using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace AppWpfSimpleClient
{
    //[KnownType(typeof(libShared.ProtolV1Commands.TestCmd))]
    [KnownType(typeof(TestCmd))]
    [DataContract]
    public class ConfigFile
    {
        public string FileName { get; set; }

        [DataMember]
        public ObservableCollection<object> CommandList { get; internal set; } = new ObservableCollection<object>();

        [DataMember]
        public ObservableCollection<object> SendList { get; internal set; } = new ObservableCollection<object>();

        [DataMember]
        public ObservableCollection<object> ReceiveList { get; internal set; } = new ObservableCollection<object>();

        public static ConfigFile Deserialize(string filename)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ConfigFile));

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(string.Format("File \"{0}\" not found", filename));
            }

            ConfigFile theclass;

            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
                {
                    theclass = (ConfigFile)serializer.ReadObject(reader, true);
                }
            }

            theclass.FileName = filename;
            return theclass;
        }

        public void Serialize(string filename)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ConfigFile));

            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
            {
                throw new DirectoryNotFoundException(string.Format("Directory \"{0}\" not found", filename));
            }
            
            using (XmlWriter stream = XmlWriter.Create(filename))
            {
                serializer.WriteObject(stream, this);
            }

            this.FileName = filename;
        }
    }
}
