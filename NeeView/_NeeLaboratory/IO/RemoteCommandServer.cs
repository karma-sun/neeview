using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// パイプを使って他のプロセスから送られてきたコマンドを受信する
    /// </summary>
    public class RemoteCommandServer : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;

        public EventHandler<RemoteCommandEventArgs> Called;


        public RemoteCommandServer()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine(GetPipetName(process));
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(ReciverAsync);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        public static string GetPipetName(Process process)
        {
            return process.ProcessName + ".p" + process.Id;
        }

        private async Task ReciverAsync()
        {
            var pipeName = GetPipetName(Process.GetCurrentProcess());

            while (true)
            {
                try
                {
                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
                    {
                        await pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);

                        using (var xreader = XmlReader.Create(pipeServer))
                        {
                            DataContractSerializer ser = new DataContractSerializer(typeof(RemoteCommand));
                            var command = ser.ReadObject(xreader) as RemoteCommand;

                            if (command != null && Called != null)
                            {
                                ////Debug.WriteLine($"Recieve: {command.ID}({string.Join(",", command.Args)})");
                                Called(this, new RemoteCommandEventArgs(command));
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            Debug.WriteLine($"Remote Server: Stopped");
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }


    /// <summary>
    /// コマンド呼び出しイベントの引数
    /// </summary>
    public class RemoteCommandEventArgs : EventArgs
    {
        public RemoteCommandEventArgs(RemoteCommand command)
        {
            Command = command;
        }

        public RemoteCommand Command { get; set; }
    }
}
