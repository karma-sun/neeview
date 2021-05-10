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

        public void Execute(object sender, string path)
        {
            _task = Task.Run(() => ExecuteInner(sender, path));
        }

        private void ExecuteInner(object sender, string path)
        {
            JavascriptEngine commandEngine = null;

            try
            {
                ////Debug.WriteLine($"Script.{path} ...");
                var commandHost = new CommandHost(sender, CommandTable.Current, ConfigMap.Current);
                commandEngine = new JavascriptEngine(commandHost);
                commandEngine.LogAction = e => Debug.WriteLine(e);
                commandEngine.ExecureFile(path, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                ////Debug.WriteLine($"Script.{path} canceld");
            }
            catch (Exception ex)
            {
                var message = CreateExceptionRecursiveMessage(ex);
                commandEngine?.Log(message);
                ToastService.Current.Show(new Toast(message, $"Script error in {Path.GetFileName(path)}", ToastIcon.Error));
                ////Debug.WriteLine($"Script.{path} failed");
            }
            finally
            {
                CommandTable.Current.FlushInputGesture();
                _pool.Remove(this);
                ////Debug.WriteLine($"Script.{path} done.");
            }

            string CreateExceptionRecursiveMessage(Exception ex)
            {
                return ex.Message + System.Environment.NewLine + (ex.InnerException != null ? CreateExceptionRecursiveMessage(ex.InnerException) : "");
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
