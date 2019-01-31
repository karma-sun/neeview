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
                // NOTE: 自プロセスは除外
                var currentProcess = Process.GetCurrentProcess();
                var processes = Process.GetProcesses().Where(e => e.ProcessName.StartsWith(_processName) && e.Id != currentProcess.Id);

                if (delivery.Type == RemoteCommandDeliveryType.Custom)
                {
                    return processes.Where(p => p.Id == delivery.ProcessId).Take(1).ToList();
                }
                else if (delivery.Type == RemoteCommandDeliveryType.Lastest)
                {
                    return processes.OrderByDescending((p) => p.StartTime).Take(1).ToList();
                }
                else
                {
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
    }

}
