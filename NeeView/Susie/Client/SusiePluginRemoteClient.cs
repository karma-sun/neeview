using NeeLaboratory;
using NeeLaboratory.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Susie.Client
{
    public class SusiePluginRemoteClient : IDisposable
    {
        private SubProcess _subProcess;
        private SimpleClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsConnected => _subProcess != null && _subProcess.IsActive;

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_subProcess != null)
                    {
                        _subProcess.Dispose();
                        _subProcess = null;
                    }

                    _client = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
        

        public void Connect()
        {
            if (_subProcess != null) return;

            var subProcessFileName = System.IO.Path.Combine(Environment.LibrariesPath, "NeeView.Susie.Server.exe");

            if (!File.Exists(subProcessFileName))
            {
                throw new FileNotFoundException($"File not found: {subProcessFileName}");
            }

            _subProcess = new SubProcess(subProcessFileName, SusiePluginRemote.BootKeyword);
            _subProcess.Start();

            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;
            _subProcess.Exited += (s, e) => cancellationTokenSource.Cancel();

            var name = SusiePluginRemote.CreateServerName(_subProcess.Process);
            _client = new SimpleClient(name);
        }

        public void Disconnect()
        {
            if (_subProcess == null) return;

            _cancellationTokenSource.Cancel();
            _subProcess.Dispose();
            _subProcess = null;
            _client = null;
        }

        public async Task<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Disconnected from SusiePlugin Server.");
            }

            using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token))
            {
                return await _client.CallAsync(args, linkedCancellationTokenSource.Token);
            }
        }
    }
}
