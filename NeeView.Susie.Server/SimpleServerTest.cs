using NeeLaboratory;
using NeeLaboratory.Runtime.Remote;
using NeeLaboratory.Runtime.Serialization;
using RemoteCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeeView.Susie.Server
{
    public class SusiePluginRemoteServer
    {
        private SimpleServer _server;
        private SusiePluginServer _process;


        public SusiePluginRemoteServer()
        {
            var name = SusiePluginRemote.CreateServerName(Process.GetCurrentProcess());
            Console.WriteLine($"ServerName: {name}");
            _server = new SimpleServer(name);
            ////_server.AddReciever(SusiePluginCommandId.Echo, Echo);
            _server.AddReciever(SusiePluginCommandId.Initialize, Initialize);
            _server.AddReciever(SusiePluginCommandId.GetPlugin, GetPlugin);
            _server.AddReciever(SusiePluginCommandId.SetPlugin, SetPlugin);
            _server.AddReciever(SusiePluginCommandId.SetPluginOrder, SetPluginOrder);
            _server.AddReciever(SusiePluginCommandId.ShowConfigulationDlg, ShowConfigulationDlg);
            _server.AddReciever(SusiePluginCommandId.GetArchivePlugin, GetArchivePlugin);
            _server.AddReciever(SusiePluginCommandId.GetImagePlugin, GetImagePlugin);
            _server.AddReciever(SusiePluginCommandId.GetImage, GetImage);
            _server.AddReciever(SusiePluginCommandId.GetArchiveEntries, GetArchiveEntries);
            _server.AddReciever(SusiePluginCommandId.ExtractArchiveEntry, ExtractArchiveEntry);
            _server.AddReciever(SusiePluginCommandId.ExtractArchiveEntryToFolder, ExtractArchiveEntryToFolder);


            _process = new SusiePluginServer();
        }

        public void Run()
        {
            _server.ServerProcess();
        }

        private TResult DeserializeChunk<TResult>(Chunk chunk)
        {
            return DefaultSerializer.Deserialize<TResult>(chunk.Data);
        }

        private List<Chunk> CreateResult<T>(int id, T result)
        {
            var chunk = new Chunk(id, DefaultSerializer.Serialize(result));
            return new List<Chunk>() { chunk };
        }

        private List<Chunk> CreateResultIsSuccess(int id, bool isSuccess)
        {
            var chunk = new Chunk(id, DefaultSerializer.Serialize(new SusiePluginCommandResult(isSuccess)));
            return new List<Chunk>() { chunk };
        }


        private List<Chunk> Echo(List<Chunk> command)
        {
            return command;
        }

        private List<Chunk> Initialize(List<Chunk> command)
        {
            Console.WriteLine($"Initialize: ...");
            var args = DeserializeChunk<SusiePluginCommandInitialize>(command[0]);
            _process.Initialize(args.PluginFolder, args.Settings);
            Console.WriteLine($"Initialize: done.");
            return CreateResultIsSuccess(SusiePluginCommandId.Initialize, true);
        }

        private List<Chunk> GetPlugin(List<Chunk> command)
        {
            Console.WriteLine($"GetPlugin: ...");
            var args = DeserializeChunk<SusiePluginCommandGetPlugin>(command[0]);
            var pluginInfos =  _process.GetPlugin(args.PluginNames);
            Console.WriteLine($"GetPlugin: done.");
            return CreateResult(SusiePluginCommandId.GetPlugin, new SusiePluginCommandGetPluginResult(pluginInfos));
        }

        private List<Chunk> SetPlugin(List<Chunk> command)
        {
            Console.WriteLine($"SetPlugin: ...");
            var args = DeserializeChunk<SusiePluginCommandSetPlugin>(command[0]);
            _process.SetPlugin(args.Settings);
            Console.WriteLine($"SetPlugin: done.");
            return CreateResultIsSuccess(SusiePluginCommandId.SetPlugin, true);
        }

        private List<Chunk> SetPluginOrder(List<Chunk> command)
        {
            Console.WriteLine($"SetPluginOrder: ...");
            var args = DeserializeChunk<SusiePluginCommandSetPluginOrder>(command[0]);
            _process.SetPluginOrder(args.Order);
            Console.WriteLine($"SetPluginOrder: done.");
            return CreateResultIsSuccess(SusiePluginCommandId.SetPluginOrder, true);
        }

        private List<Chunk> ShowConfigulationDlg(List<Chunk> command)
        {
            Console.WriteLine($"ShowConfigulationDlg: ...");
            var args = DeserializeChunk<SusiePluginCommandShowConfigulationDlg>(command[0]);
            _process.ShowConfigulationDlg(args.PluginName, args.HWnd);
            Console.WriteLine($"ShowConfigulationDlg: done.");
            return CreateResultIsSuccess(SusiePluginCommandId.ShowConfigulationDlg, true);
        }


        private List<Chunk> GetArchivePlugin(List<Chunk> command)
        {
            Console.WriteLine($"GetArchivePlugin: ...");
            var args = DeserializeChunk<SusiePluginCommandGetArchivePlugin>(command[0]);
            var buff = command[1].Data;
            var pluginInfo = _process.GetArchivePlugin(args.FileName, buff, args.IsCheckExtension);
            Console.WriteLine($"GetArchivePlugin: done.");
            return CreateResult(SusiePluginCommandId.GetArchivePlugin, new SusiePluginCommandGetArchivePluginResult(pluginInfo));
        }

        private List<Chunk> GetImagePlugin(List<Chunk> command)
        {
            Console.WriteLine($"GetImagePlugin: ...");
            var args = DeserializeChunk<SusiePluginCommandGetImagePlugin>(command[0]);
            var buff = command[1].Data;
            var pluginInfo = _process.GetImagePlugin(args.FileName, buff, args.IsCheckExtension);
            Console.WriteLine($"GetImagePlugin: done.");
            return CreateResult(SusiePluginCommandId.GetImagePlugin, new SusiePluginCommandGetImagePluginResult(pluginInfo));
        }

        private List<Chunk> GetImage(List<Chunk> command)
        {
            Console.WriteLine($"GetImage: ...");
            var args = DeserializeChunk<SusiePluginCommandGetImage>(command[0]);
            var buff = command[1].Data;
            var susieImage = _process.GetImage(args.PluginName, args.FileName, buff, args.IsCheckExtension);
            Console.WriteLine($"GetImage: done.");
            var result =  CreateResult(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImageResult(susieImage.Plugin));
            result.Add(new Chunk(SusiePluginCommandId.GetImage, susieImage.BitmapData));
            return result;
        }


        private List<Chunk> GetArchiveEntries(List<Chunk> command)
        {
            Console.WriteLine($"GetArchiveEntries: ...");
            var args = DeserializeChunk<SusiePluginCommandGetArchiveEntries>(command[0]);
            var entries = _process.GetArchiveEntries(args.PluginName, args.FileName);
            Console.WriteLine($"GetArchiveEntries: done.");
            return CreateResult(SusiePluginCommandId.GetArchiveEntries, new SusiePluginCommandGetArchiveEntriesResult(entries));
        }

        private List<Chunk> ExtractArchiveEntry(List<Chunk> command)
        {
            Console.WriteLine($"ExtractArchiveEntry: ...");
            var args = DeserializeChunk<SusiePluginCommandExtractArchiveEntry>(command[0]);
            var buff = _process.ExtractArchiveEntry(args.PluginName, args.FileName, args.Position);
            Console.WriteLine($"ExtractArchiveEntry: done.");
            return new List<Chunk>() { new Chunk(SusiePluginCommandId.ExtractArchiveEntry, buff) };
        }

        private List<Chunk> ExtractArchiveEntryToFolder(List<Chunk> command)
        {
            Console.WriteLine($"ExtractArchiveEntryToFolder: ...");
            var args = DeserializeChunk<SusiePluginCommandExtractArchiveEntryToFolder>(command[0]);
            _process.ExtractArchiveEntryToFolder(args.PluginName, args.FileName, args.Position, args.ExtractFolder);
            Console.WriteLine($"ExtractArchiveEntryToFolder: done.");
            return CreateResultIsSuccess(SusiePluginCommandId.ExtractArchiveEntryToFolder, true);
        }
    }
}

#if false
namespace RemoteProcessServer
{
    public class SimpleServerTest
    {
        private SimpleServer _server;

        public SimpleServerTest()
        {
            var name = SimpleServerUtility.CreateServerName(Process.GetCurrentProcess());
            _server = new SimpleServer(name);
            _server.AddReciever(SimpleCommandId.Echo, Echo);
            _server.AddReciever(SimpleCommandId.Add, Add);
            _server.AddReciever(SimpleCommandId.BitmapLoad, BitmapLoad);
        }

        public void Run()
        {
            _server.ServerProcess();
        }

        private Chunk Echo(Chunk command)
        {
            return command;
        }

        private Chunk Add(Chunk command)
        {
            var args = DefaultSerializer.Deserialize<SimpleCommandAddArgs>(command.Data);
            var result = new Chunk(SimpleCommandId.Add, DefaultSerializer.Serialize(new SimpleCommandAddResult() { Answer = args.X + args.Y }));

            ////MessageBox.Show($"Answer: {args.X + args.Y}");

            return result;
        }

        private Chunk BitmapLoad(Chunk command)
        {
            var filename = DefaultSerializer.Deserialize<string>(command.Data);
            var data = File.ReadAllBytes(filename);
            var result = new Chunk(SimpleCommandId.BitmapLoad, data);
            return result;
        }
    }
}
#endif
