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
    //[KnownType(typeof(libShared.ProtolV1Commands.ProtocolV1Base))]
    //[KnownType(typeof(libShared.ProtolV1Commands.TestCmd))]
    //[KnownType(typeof(libSharedProject.ProtolV1Commands.ProtocolV1Base))]
    //[KnownType(typeof(libSharedProject.ProtolV1Commands.TestCmd))]
    [KnownType(typeof(DataSet1.DataTableCmdDataTable))]
    [DataContract]
    public class ConfigFile
    {
        public string FileName { get; set; }

        //[DataMember]
        //public ObservableCollection<KeyValuePair<string, object>> CommandList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();
          
        //[DataMember]
        //public ObservableCollection<KeyValuePair<string, object>> SendList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();

        //[DataMember]
        //public ObservableCollection<KeyValuePair<string, object>> ReceiveList { get; internal set; } = new ObservableCollection<KeyValuePair<string, object>>();

        [DataMember]
        public System.Data.DataTable CommandTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();

        [DataMember]
        public System.Data.DataTable SendTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();

        [DataMember]
        public System.Data.DataTable ReceiveTable { get; internal set; } = new DataSet1.DataTableCmdDataTable();

        public static ConfigFile Deserialize(string filename)
        {

            //ConfigFile theclass = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(filename));

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
            //File.WriteAllText(filename, Newtonsoft.Json.JsonConvert.SerializeObject(this));

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
