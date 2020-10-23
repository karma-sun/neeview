using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// パイプを使って他のプロセスにコマンドを送る
    /// </summary>
    public class RemoteCommandClient
    {
        private string _processName;


        public RemoteCommandClient(string processName)
        {
            _processName = processName;
        }


        public async Task SendAsync(RemoteCommand command, RemoteCommandDelivery delivery)
        {
            var processes = await CollectProcess(delivery);
            foreach (var process in processes)
            {
                await SendAsync(RemoteCommandServer.GetPipetName(process), command);
            }
        }

        private async Task<List<Process>> CollectProcess(RemoteCommandDelivery delivery)
        {
            return await Task.Run(() =>
            {
                var currentProcess = Process.GetCurrentProcess();

                // collect NeeView processes
                var processes = Process.GetProcesses().Where(e => e.ProcessName == _processName).ToList();

                // 自身を基準として並び替え。自身は削除する
                var index = processes.FindIndex(e => e.Id == currentProcess.Id);
                processes = processes.Skip(index).Concat(processes.Take(index)).Where(e => e.Id != currentProcess.Id).ToList();

                switch (delivery.Type)
                {
                    case RemoteCommandDeliveryType.Custom:
                        return processes.Where(p => p.Id == delivery.ProcessId).Take(1).ToList();

                    case RemoteCommandDeliveryType.Lastest:
                        return processes.OrderByDescending((p) => p.StartTime).Take(1).ToList();

                    case RemoteCommandDeliveryType.Previous:
                        return ((IEnumerable<Process>)processes).Reverse().Take(1).ToList();

                    case RemoteCommandDeliveryType.Next:
                        return processes.Take(1).ToList();

                    default:
                        return processes.ToList();
                }
            });
        }

        private async Task SendAsync(string pipeName, RemoteCommand command)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                await pipeClient.ConnectAsync(500);
                using (var writer = XmlWriter.Create(pipeClient))
                {
                    var serializer = new DataContractSerializer(typeof(RemoteCommand));
                    serializer.WriteObject(writer, command);
                }
            }
        }
    }


    public class RemoteCommandDelivery
    {
        public static RemoteCommandDelivery All { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.All);
        public static RemoteCommandDelivery Lastest { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Lastest);
        public static RemoteCommandDelivery Previous { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Previous);
        public static RemoteCommandDelivery Next { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Next);


        public RemoteCommandDelivery(RemoteCommandDeliveryType type)
        {
            Type = type;
        }

        public RemoteCommandDelivery(int processId)
        {
            Type = RemoteCommandDeliveryType.Custom;
            ProcessId = processId;
        }

        public RemoteCommandDeliveryType Type { get; private set; }
        public int ProcessId { get; private set; }
    }

    /// <summary>
    /// 配信先ターゲット
    /// </summary>
    public enum RemoteCommandDeliveryType
    {
        /// <summary>
        /// 自身を除く全プロセス
        /// </summary>
        All,

        /// <summary>
        /// 自身を除く最新プロセス
        /// </summary>
        Lastest,

        /// <summary>
        /// 指定のプロセス
        /// </summary>
        Custom,

        /// <summary>
        /// 前のプロセス
        /// </summary>
        Previous,

        /// <summary>
        /// 次のプロセス
        /// </summary>
        Next,
    }

}
