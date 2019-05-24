using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace RemoteCommon
{
    public static class SimpleServerUtility
    {
        public static string CreateServerName(Process process)
        {
            return $"nv{process.Id}.rpc";
        }
    }

    public static class SimpleCommandId
    {
        public const int None = 0x0000;
        public const int Echo = 0x0001;
        public const int Add = 0x0002;
        public const int BitmapLoad = 0x0003;

        public const int Error = -1;
    }


    [DataContract]
    public class SimpleCommandAddArgs
    {
        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }
    }


    [DataContract]
    public class SimpleCommandAddResult
    {
        [DataMember]
        public int Answer { get; set; }
    }


}
