using NeeLaboratory;
using NeeLaboratory.Runtime.Remote;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Susie.Client
{
    public class SusiePluginRemoteClient : IDisposable
    {
        private SubProcess _subProcess;
        private SimpleClient _client;

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

            // TODO: Libraryパスの反映
            _subProcess = new SubProcess(@"NeeView.Susie.Server.exe");
            _subProcess.Start();

            var name = SusiePluginRemote.CreateServerName(_subProcess.Process);
            _client = new SimpleClient(name);
        }

        public void Disconnect()
        {
            if (_subProcess == null) return;

            _subProcess.Dispose();
            _subProcess = null;
            _client = null;
        }

        public async Task<List<Chunk>> CallAsync(Chunk args, CancellationToken token)
        {
            return await CallAsync(new List<Chunk>() { args }, token);
        }

        public async Task<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            if (_client == null) throw new InvalidOperationException();

            RETRY:
            try
            {
                return await _client.CallAsync(args, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");

#if false
                var dialog = new MessageDialog("Susieプラグインサーバーのエラーです。リトライしますか？", "プラグインエラー");
                dialog.Commands.Add(UICommands.Retry);
                dialog.Commands.Add(UICommands.Cancel);
                var result = dialog.ShowDialog();

                if (result == UICommands.Retry)
                {
                    // TODO: 初期化からやり直す必要がある。再接続ではダメ
                    Connect();
                    goto RETRY;
                }
#endif

                throw;
            }
        }
    }


    public class SusiePluginClient : IRemoteSusiePlugin, IDisposable
    {
        private SusiePluginRemoteClient _remote;

        public SusiePluginClient(SusiePluginRemoteClient remote)
        {
            _remote = remote;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private async Task<List<Chunk>> CallAsync<T>(int id, T arg, CancellationToken token)
        {
            var chunk = new Chunk(id, DefaultSerializer.Serialize(arg));
            return await _remote.CallAsync(chunk, token);
        }

        private async Task<List<Chunk>> CallAsync<T>(int id, T arg1, byte[] arg2, CancellationToken token)
        {
            var chunk1 = new Chunk(id, DefaultSerializer.Serialize(arg1));
            var chunk2 = new Chunk(id, arg2);
            return await _remote.CallAsync(new List<Chunk>() { chunk1, chunk2 }, token);
        }

        private TResult DeserializeChunk<TResult>(Chunk chunk)
        {
            return DefaultSerializer.Deserialize<TResult>(chunk.Data);
        }


        public void Initialize(string pluginFolder, List<SusiePluginSetting> settings)
        {
            var task = Task.Run(async () =>
            {
                _remote.Connect();
                await CallAsync(SusiePluginCommandId.Initialize, new SusiePluginCommandInitialize(pluginFolder, settings), CancellationToken.None);
            });

            task.Wait();
        }

        public List<SusiePluginInfo> GetPlugin(List<string> pluginNames)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.GetPlugin, new SusiePluginCommandGetPlugin(pluginNames), CancellationToken.None);
                return DeserializeChunk<SusiePluginCommandGetPluginResult>(chunks[0]).PluginInfos;
            });

            return task.Result;
        }

        public void SetPlugin(List<SusiePluginSetting> settings)
        {
            var task = Task.Run(async () =>
            {
                await CallAsync(SusiePluginCommandId.SetPlugin, new SusiePluginCommandSetPlugin(settings), CancellationToken.None);
            });

            task.Wait();
        }


        public void SetPluginOrder(List<string> order)
        {
            var task = Task.Run(async () =>
            {
                await CallAsync(SusiePluginCommandId.SetPluginOrder, new SusiePluginCommandSetPluginOrder(order), CancellationToken.None);
            });

            task.Wait();
        }

        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            var task = Task.Run(async () =>
            {
                await CallAsync(SusiePluginCommandId.ShowConfigulationDlg, new SusiePluginCommandShowConfigulationDlg(pluginName, hwnd), CancellationToken.None);
            });

            task.Wait();
        }

        public SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.GetArchivePlugin, new SusiePluginCommandGetArchivePlugin(fileName, isCheckExtension), buff, CancellationToken.None);
                return DeserializeChunk<SusiePluginCommandGetArchivePluginResult>(chunks[0]).PluginInfo;
            });

            return task.Result;
        }

        public SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImagePlugin(fileName, isCheckExtension), buff, CancellationToken.None);
                return DeserializeChunk<SusiePluginCommandGetImagePluginResult>(chunks[0]).PluginInfo;
            });

            return task.Result;
        }

        public SusieImage GetImage(string pluginName, string fileName, byte[] buff, bool isCheckExtension)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImage(pluginName, fileName, isCheckExtension), buff, CancellationToken.None);
                return new SusieImage(DeserializeChunk<SusiePluginCommandGetImageResult>(chunks[0]).PluginInfo, chunks[1].Data);
            });

            return task.Result;
        }


        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.GetArchiveEntries, new SusiePluginCommandGetArchiveEntries(pluginName, fileName), CancellationToken.None);
                return DeserializeChunk<SusiePluginCommandGetArchiveEntriesResult>(chunks[0]).Entries;
            });

            return task.Result;
        }

        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            var task = Task.Run(async () =>
            {
                var chunks = await CallAsync(SusiePluginCommandId.ExtractArchiveEntry, new SusiePluginCommandExtractArchiveEntry(pluginName, fileName, position), CancellationToken.None);
                return chunks[0].Data;
            });

            return task.Result;
        }

        public void ExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            var task = Task.Run(async () =>
            {
                await CallAsync(SusiePluginCommandId.ExtractArchiveEntryToFolder, new SusiePluginCommandExtractArchiveEntryToFolder(pluginName, fileName, position, extractFolder), CancellationToken.None);
            });

            task.Wait();
        }


  


    }
}
