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
            // セマフォで排他処理。通信は同時に１つだけ
            _semaphore.Wait();
            try
            {
                // 接続１秒タイムアウトで１０回リトライ
                for (int retry = 0; retry < 10; ++retry)
                {
                    try
                    {
                        return await CallInnerAsync(args, 1000, token);
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine($"Client: Connect {_serverPipeName} timeout. Retry!");
                    }
                }

                throw new TimeoutException();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Chunk>> CallInnerAsync(List<Chunk> args, int timeout, CancellationToken token)
        {
            ////Debug.WriteLine($"Client: Start");
            using (var pipeClient = new NamedPipeClientStream(".", _serverPipeName, PipeDirection.InOut))
            {
                ////Debug.WriteLine($"Client: Connect {_serverPipeName} ...");
                await pipeClient.ConnectAsync(timeout, token);

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
    }
}


