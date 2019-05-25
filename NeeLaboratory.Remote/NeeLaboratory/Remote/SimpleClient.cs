using NeeLaboratory.IO;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Remote
{
    public class SimpleClient
    {
        private string _serverPipeName;


        public SimpleClient(string serverPipeName)
        {
            _serverPipeName = serverPipeName;
        }

        public async Task<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            Console.WriteLine($"Client: Start");
            using (var pipeClient = new NamedPipeClientStream(".", _serverPipeName, PipeDirection.InOut))
            {
                Console.WriteLine($"Client: Connect {_serverPipeName} ...");
                await pipeClient.ConnectAsync(3000, token);

                using (var stream = new ChunkStream(pipeClient, true))
                {
                    // call
                    Console.WriteLine($"Client: Call: {args[0].Id}");
                    stream.WriteChunkArray(args);

                    // result
                    var result = await stream.ReadChunkArrayAsync(token);
                    Console.WriteLine($"Client: Result.Recv: {result[0].Id}");

                    if (result[0].Id < 0)
                    {
                        throw new IOException(DefaultSerializer.Deserialize<string>(result[0].Data));
                    }

                    return result;
                }
            }
        }
    }
}


