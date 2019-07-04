using System.Runtime.Serialization;
using System.Text;

namespace NeeLaboratory.IO
{

    [DataContract]
    public class RemoteCommand
    {
        public RemoteCommand(string id)
        {
            Id = id;
        }

        public RemoteCommand(string id, params string[] args)
        {
            Id = id;
            Args = args;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string[] Args { get; set; }
    }

}
