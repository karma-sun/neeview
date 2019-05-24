using NeeLaboratory;
using NeeLaboratory.Diagnostics;
using NeeLaboratory.Runtime.Remote;
using NeeLaboratory.Runtime.Serialization;
using NeeView;
using RemoteCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RemoteProcessClient
{
#if false
    public static class RemoteClientTest
    {
        ////private static ProcessJobObject _processJobObject;

        public static async Task TestAsync()
        {
            ////_processJobObject = new ProcessJobObject();
            ////_processJobObject.AddProcess(Process.GetCurrentProcess().Handle);


            var cancellationTokenSource = new CancellationTokenSource();

            Debug.WriteLine($"I'm Client.");

            await new SimpleClientTest().ExecuteAsync(cancellationTokenSource.Token);

            Debug.WriteLine($"Done.");
        }
    }


    public class RemoteClient
    {
        private SubProcess _subProcess;
        private SimpleClient _client;

        public RemoteClient()
        {
            Connect();
        }

        private void Connect()
        {
            if (_subProcess != null)
            {
                _subProcess.Dispose();
            }

            _subProcess = new SubProcess(@"NeeView.Susie.Server.exe");
            _subProcess.Start();

            var name = SimpleServerUtility.CreateServerName(_subProcess.Process);
            _client = new SimpleClient(name);
        }

        public async Task<Chunk> CallAsync(Chunk args, CancellationToken token)
        {
        RETRY:
            try
            {
                return await _client.CallAsync(args, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                Console.WriteLine("Susieプラグインサーバーのエラーです。リトライしますか？ (Y or n)");
                var key = Console.ReadKey();

                if (key.Key != ConsoleKey.N)
                {
                    Connect();
                    goto RETRY;
                }

                throw;
            }
        }
    }


    public partial class SimpleClientTest
    {
        private RemoteClient _client;

        public SimpleClientTest()
        {
            _client = new RemoteClient();
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            for (int i = 0; i < 20; i++)
            {
                //await CommandEcho("Hello", token);
                {
                    var sw = Stopwatch.StartNew();
                    await CommandAdd(1, 2, token);
                    await CommandEcho(new byte[1024 * 1024 * 100], token);
                    await CommandBitmapLoad(@"E:\Pictures\012.jpg", token);
                    //await CommandBitmapLoad(@"E:\Work\Labo\サンプル画像\巨大画像.jpg", token);

                    Console.WriteLine($"ExitTime:A: {sw.ElapsedMilliseconds}ms");
                }


                await Task.Delay(500);
            }
        }



        public async Task<byte[]> CommandEcho(byte[] data, CancellationToken token)
        {
            Console.WriteLine($"Echo: {data.Length:#,0}");
            var args = new Chunk(SimpleCommandId.Echo, data);
            var result = (await _client.CallAsync(args, token)).Data;
            Console.WriteLine($"Echo.Result: {result.Length:#,0}");
            return result;
        }

        public async Task<int> CommandAdd(int x, int y, CancellationToken token)
        {
            Console.WriteLine($"Add: {x}, {y}");
            var args = new Chunk(SimpleCommandId.Add, DefaultSerializer.Serialize(new SimpleCommandAddArgs() { X = x, Y = y }));
            var result = DefaultSerializer.Deserialize<SimpleCommandAddResult>((await _client.CallAsync(args, token)).Data);
            Console.WriteLine($"Add.Result: {result.Answer}");
            return result.Answer;
        }


        public async Task<byte[]> CommandBitmapLoad(string filename, CancellationToken token)
        {
            Console.WriteLine($"BitmapLoad: {filename}");
            var args = new Chunk(SimpleCommandId.BitmapLoad, DefaultSerializer.Serialize(filename));
            var result = (await _client.CallAsync(args, token)).Data;

            using (var stream = new MemoryStream(result))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                Console.WriteLine($"BitmapLoad: {result.Length:#,0}byte, Size={bitmap.PixelWidth}x{bitmap.PixelHeight}");
            }

            return result;
        }

    }

#endif
}
