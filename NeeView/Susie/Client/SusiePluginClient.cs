using NeeLaboratory;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin
    {
        private SusiePluginRemoteClient _remote;
        private Action _recoveryAction;
        private bool _isRecoveryDoing;

        public SusiePluginClient(SusiePluginRemoteClient remote)
        {
            _remote = remote;
        }


        public void SetRecoveryAction(Action action)
        {
            _recoveryAction = action;
        }

        private void Recovery()
        {
            if (_isRecoveryDoing) return;

            _isRecoveryDoing = true;
            try
            {
                Debug.WriteLine($"SuisePluginCluent: Recovery...");
                _remote.Disconnect();
                _remote.Connect();
                _recoveryAction?.Invoke();
            }
            finally
            {
                _isRecoveryDoing = false;
            }
        }


        private List<Chunk> Call<T>(int id, T arg)
        {
            var chunk = new Chunk(id, DefaultSerializer.Serialize(arg));
            return Call(new List<Chunk>() { chunk });
        }

        private List<Chunk> Call<T>(int id, T arg1, byte[] arg2)
        {
            var chunk1 = new Chunk(id, DefaultSerializer.Serialize(arg1));
            var chunk2 = new Chunk(id, arg2);
            return Call(new List<Chunk>() { chunk1, chunk2 });
        }

        private List<Chunk> Call(List<Chunk> args)
        {
            if (!_remote.IsConnected)
            {
                Recovery();
            }

            var task = Task.Run(async () => await _remote.CallAsync(args, CancellationToken.None));

            try
            {
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        private TResult DeserializeChunk<TResult>(Chunk chunk)
        {
            return DefaultSerializer.Deserialize<TResult>(chunk.Data);
        }



        public void Initialize(string pluginFolder, List<SusiePluginSetting> settings)
        {
            _remote.Connect();
            Call(SusiePluginCommandId.Initialize, new SusiePluginCommandInitialize(pluginFolder, settings));
        }

        public List<SusiePluginInfo> GetPlugin(List<string> pluginNames)
        {
            var chunks = Call(SusiePluginCommandId.GetPlugin, new SusiePluginCommandGetPlugin(pluginNames));
            return DeserializeChunk<SusiePluginCommandGetPluginResult>(chunks[0]).PluginInfos;
        }

        public void SetPlugin(List<SusiePluginSetting> settings)
        {
            Call(SusiePluginCommandId.SetPlugin, new SusiePluginCommandSetPlugin(settings));
        }

        public void SetPluginOrder(List<string> order)
        {
            Call(SusiePluginCommandId.SetPluginOrder, new SusiePluginCommandSetPluginOrder(order));
        }

        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            Call(SusiePluginCommandId.ShowConfigulationDlg, new SusiePluginCommandShowConfigulationDlg(pluginName, hwnd));
        }

        public SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetArchivePlugin, new SusiePluginCommandGetArchivePlugin(fileName, isCheckExtension), buff);
            return DeserializeChunk<SusiePluginCommandGetArchivePluginResult>(chunks[0]).PluginInfo;
        }

        public SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImagePlugin(fileName, isCheckExtension), buff);
            return DeserializeChunk<SusiePluginCommandGetImagePluginResult>(chunks[0]).PluginInfo;
        }

        public SusieImage GetImage(string pluginName, string fileName, byte[] buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImage(pluginName, fileName, isCheckExtension), buff);
            return new SusieImage(DeserializeChunk<SusiePluginCommandGetImageResult>(chunks[0]).PluginInfo, chunks[1].Data);
        }

        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            var chunks = Call(SusiePluginCommandId.GetArchiveEntries, new SusiePluginCommandGetArchiveEntries(pluginName, fileName));
            return DeserializeChunk<SusiePluginCommandGetArchiveEntriesResult>(chunks[0]).Entries;
        }

        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            var chunks = Call(SusiePluginCommandId.ExtractArchiveEntry, new SusiePluginCommandExtractArchiveEntry(pluginName, fileName, position));
            return chunks[0].Data;
        }

        public void ExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            Call(SusiePluginCommandId.ExtractArchiveEntryToFolder, new SusiePluginCommandExtractArchiveEntryToFolder(pluginName, fileName, position, extractFolder));
        }
    }
}
