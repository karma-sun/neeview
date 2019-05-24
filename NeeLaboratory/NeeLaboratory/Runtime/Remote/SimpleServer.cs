using NeeLaboratory.IO;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Runtime.Remote
{
    public class SimpleServer
    {
        public delegate List<Chunk> SimpleReciever(List<Chunk> chunks);

        private string _name;

        private Dictionary<int, SimpleReciever> _recievers = new Dictionary<int, SimpleReciever>();

        public SimpleServer(string name)
        {
            _name = name;
        }

        public void AddReciever(int id, SimpleReciever reciever)
        {
            _recievers.Add(id, reciever);
        }

        public void ServerProcess()
        {
            while (true)
            {
                ServerProcessTurn();
                GC.Collect();
            }
        }

        public void ServerProcessTurn()
        {
            Console.WriteLine($"Server: Start");
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(_name, PipeDirection.InOut))
            {
                Console.WriteLine($"Server: Wait for connect...");
                pipeServer.WaitForConnection();

                using (var stream = new ChunkStream(pipeServer, true))
                {
                    // read
                    var command = stream.ReadChunkArray();
                    Console.WriteLine($"Server: Call.Recv: ChunkCount={command.Count}");

                    // execute
                    var result = CommandExecute(command);

                    // write
                    Console.WriteLine($"Server: Result: ChunkCount={result.Count}");
                    stream.WriteChunkArray(result);
                }
            }

            Console.WriteLine($"Server: Closed");
        }

        private List<Chunk> CommandExecute(List<Chunk> command)
        {
            try
            {
                return _recievers[command[0].Id].Invoke(command);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<Chunk>() { new Chunk(-1, DefaultSerializer.Serialize(ex.Message)) };
            }
        }

    }
}
