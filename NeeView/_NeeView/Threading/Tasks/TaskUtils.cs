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
        //
        public static Task ActionAsync(Action action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action());
        }

        //
        public static Task ActionAsync(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action(token));
        }

        //
        public static Task WaitAsync(Task task)
        {
            return ActionAsync(() => task.Wait(), CancellationToken.None);
        }

        //
        public static Task WaitAsync(Task task, CancellationToken token)
        {
            return ActionAsync(() => task.Wait(token), token);
        }
    }
}
