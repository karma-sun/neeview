using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ScriptUnit
    {
        private ScriptUnitPool _pool;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ScriptUnit(ScriptUnitPool pool)
        {
            if (pool is null) throw new ArgumentNullException();

            _pool = pool;
        }

        public void Execute(object sender, string path, string argument)
        {
            _task = Task.Run(() => ExecuteInner(sender, path, argument));
        }

        private void ExecuteInner(object sender, string path, string argument)
        {
            var engine = new JavascriptEngine() { IsToastEnable = true };

            try
            {
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} ...");
                engine.ExecureFile(path, argument, _cancellationTokenSource.Token);
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} done.");
            }
            catch (Exception ex)
            {
                engine.ExceptionPrcess(ex);
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} stopped.");
            }
            finally
            {
                CommandTable.Current.FlushInputGesture();
                _pool.Remove(this);
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
