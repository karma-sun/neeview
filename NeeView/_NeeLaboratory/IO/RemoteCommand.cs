using System.Runtime.Serialization;
using System.Text;

namespace NeeLaboratory.IO
{

    [DataContract]
    public class RemoteCommand
    {
        public RemoteCommand(string iD)
        {
            ID = iD;
        }

        public RemoteCommand(string iD, params string[] args)
        {
            ID = iD;
            Args = args;
        }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string[] Args { get; set; }
    }

}
