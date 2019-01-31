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
        public async Task SendAsync(RemoteCommand command, RemoteCommandDelivery target)
        {
            var processes = await CollectProcess(target);
            foreach (var process in processes)
            {
                await SendAsync(RemoteCommandServer.GetPipetName(process), command);
            }
        }

        private async Task<List<Process>> CollectProcess(RemoteCommandDelivery target)
        {
            var processes = await Task.Run(() =>
            {
                // NOTE: NeeView,NeeViewSどちらのプロセスにも対応。自プロセスは除外
                var currentProcess = Process.GetCurrentProcess();
                return Process.GetProcesses().Where(e => e.ProcessName.StartsWith(currentProcess.ProcessName) && e.Id != currentProcess.Id);
            });

            if (target == RemoteCommandDelivery.Lastest)
            {
                // 最も古いプロセスを返す
                var main = processes.OrderByDescending((p) => p.StartTime).FirstOrDefault();
                return main != null ? new List<Process>() { main } : new List<Process>();
            }
            else
            {
                return processes.ToList();
            }
        }

        private async Task SendAsync(string pipeName, RemoteCommand command)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                await pipeClient.ConnectAsync(500);
                using (var xwriter = XmlWriter.Create(pipeClient))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(RemoteCommand));
                    ser.WriteObject(xwriter, command);
                }
            }
        }
    }


    /// <summary>
    /// 配信先ターゲット
    /// </summary>
    public enum RemoteCommandDelivery
    {
        All,
        Lastest,
    }

}
