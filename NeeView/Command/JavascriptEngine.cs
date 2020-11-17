using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace NeeView
{
    public class JavascriptEngine
    {
        private Jint.Engine _engine;
        private CommandHost _commandHost;
        private CancellationToken _cancellationToken;


        public JavascriptEngine(CommandHost commandHost)
        {
            _commandHost = commandHost;

            _engine = new Jint.Engine(config => config.AllowClr());
            _engine.SetValue("sleep", (Action<int>)Sleep);
            _engine.SetValue("log", (Action<object>)Log);
            _engine.SetValue("system", (Action<string, string>)SystemCall);
            _engine.SetValue("include", (Func<string, object>)ExecureFile);
            _engine.SetValue("nv", _commandHost);
        }

        public string CurrentPath { get; set; }

        public Action<object> LogAction { get; set; } = Console.WriteLine;


        private void Sleep(int millisecond)
        {
            if (_cancellationToken.WaitHandle.WaitOne(millisecond))
            {
                throw new OperationCanceledException();
            }
        }

        public void Log(object log)
        {
            LogAction?.Invoke(log);
        }

        private void SystemCall(string filename, string args = null)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = filename;
            startInfo.Arguments = args;
            Process.Start(startInfo);
        }

        private object ExecureFile(string path)
        {
            return ExecureFile(path, _cancellationToken);
        }

        public object ExecureFile(string path, CancellationToken token)
        {
            var fullpath = GetFullPath(path);
            string script = File.ReadAllText(fullpath, Encoding.UTF8);

            var oldPath = CurrentPath;
            try
            {
                CurrentPath = Path.GetDirectoryName(fullpath);
                return Execute(script, token);
            }
            finally
            {
                CurrentPath = oldPath;
            }
        }


        public object Execute(string script, CancellationToken token)
        {
            _cancellationToken = token;

            var result = _engine.Execute(script).GetCompletionValue();
            return result?.ToObject();
        }

        public void SetValue(string name, object value)
        {
            _engine.SetValue(name, value);
        }

        public object GetValue(string name)
        {
            return _engine.GetValue(name).ToObject();
        }

        private string GetFullPath(string path)
        {
            if (CurrentPath != null && !Path.IsPathRooted(path))
            {
                path = Path.Combine(CurrentPath, path);
            }

            return Path.GetFullPath(path);
        }
    }

}
