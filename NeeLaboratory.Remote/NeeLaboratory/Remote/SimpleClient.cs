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
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);


        public SimpleClient(string serverPipeName)
        {
            _serverPipeName = serverPipeName;
        }

        public async Task<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            _semaphore.Wait();
            try
            {
                ////Debug.WriteLine($"Client: Start");
                using (var pipeClient = new NamedPipeClientStream(".", _serverPipeName, PipeDirection.InOut))
                {
                    ////Debug.WriteLine($"Client: Connect {_serverPipeName} ...");
                    await pipeClient.ConnectAsync(5000, token);

                    using (var stream = new ChunkStream(pipeClient, true))
                    {
                        // call
                        ////Debug.WriteLine($"Client: Call: {args[0].Id}");
                        stream.WriteChunkArray(args);

                        // result
                        var result = await stream.ReadChunkArrayAsync(token);
                        ////Debug.WriteLine($"Client: Result.Recv: {result[0].Id}");

                        if (result[0].Id < 0)
                        {
                            throw new IOException(DefaultSerializer.Deserialize<string>(result[0].Data));
                        }

                        return result;
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}


