using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Threading.Tasks
{
    public static class TaskUtils
    {
        public static Task ActionAsync(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action(token));
        }

        public static async Task WaitAsync(Task task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await Task.Run(() => task.Wait(token));
        }

        /// <summary>
        /// WaitHandle待ちのタスク化。
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ja-jp/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types
        /// </remarks>
        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null) throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate { tcs.TrySetResult(true); }, null, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }


    }
}
