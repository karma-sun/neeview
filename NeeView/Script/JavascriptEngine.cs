using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NeeView
{
    public class JavascriptEngine
    {
        private Jint.Engine _engine;
        private ScriptAccessDiagnostics _accessDiagnostics;
        private CommandHost _commandHost;
        private CancellationToken _cancellationToken;

        public JavascriptEngine()
        {
            _accessDiagnostics = new ScriptAccessDiagnostics(this);
            _commandHost = new CommandHost(this, _accessDiagnostics, CommandTable.Current);
            _engine = new Jint.Engine(config => config.AllowClr());
            _engine.SetValue("sleep", (Action<int>)Sleep);
            _engine.SetValue("log", (Action<object>)Log);
            _engine.SetValue("system", (Action<string, string>)SystemCall);
            _engine.SetValue("include", (Func<string, object>)ExecureFile);
            _engine.SetValue("nv", _commandHost);
        }

        public string CurrentPath { get; private set; }

        public string CurrentFolder { get; set; }

        public bool IsToastEnable { get; set; }


        [Documentable(Name = "include")]
        public object ExecureFile(string path)
        {
            return ExecureFile(path, null, _cancellationToken);
        }

        public object ExecureFile(string path, string argument, CancellationToken token)
        {
            var fullpath = GetFullPath(path);
            string script = File.ReadAllText(fullpath, Encoding.UTF8);

            var oldFolder = CurrentFolder;
            var oldArgs = _commandHost.Args;
            try
            {
                CurrentFolder = Path.GetDirectoryName(fullpath);
                _commandHost.SetArgs(StringTools.SplitArgument(argument));
                return Execute(fullpath, script, token);
            }
            finally
            {
                CurrentFolder = oldFolder;
                _commandHost.SetArgs(oldArgs);
            }
        }

        public object Execute(string path, string script, CancellationToken token)
        {
            _cancellationToken = token;
            _commandHost.SetCancellationToken(token);

            var oldPath = path;
            try
            {
                CurrentPath = path;
                var result = _engine.Execute(script).GetCompletionValue();
                return result?.ToObject();
            }
            finally
            {
                CurrentPath = oldPath;
            }
        }

        public void ExceptionPrcess(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return;
            }

            var message = CreateMessageWithLocation(ex.Message);
            ConsoleWindowManager.Current.ErrorMessage(message, this.IsToastEnable);
        }

        [Documentable(Name = "log")]
        public void Log(object log)
        {
            var message = log as string ?? new JsonStringBulder(log).ToString();
            ConsoleWindowManager.Current.WriteLine(ScriptMessageLevel.None, message);
        }

        [Documentable(Name = "sleep")]
        public void Sleep(int millisecond)
        {
            if (_cancellationToken.WaitHandle.WaitOne(millisecond))
            {
                throw new OperationCanceledException();
            }
        }

        [Documentable(Name = "system")]
        public void SystemCall(string filename, string args = null)
        {
            ExternalProcess.Start(filename, args, ExternalProcessAtrtibute.ThrowException);
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
            if (CurrentFolder != null && !Path.IsPathRooted(path))
            {
                path = Path.Combine(CurrentFolder, path);
            }

            return Path.GetFullPath(path);
        }

        public string CreateMessageWithLocation(string s)
        {
            // NOTE: GetLastSyntaxNode() はバージョンによっては使用できないので注意
            var node = _engine.GetLastSyntaxNode();

            int line = -1;
            string message = "";

            var regex = new Regex(@"^Line\s*(\d+):(.+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                line = int.Parse(match.Groups[1].Value);
                message = match.Groups[2].Value.Trim();
            }
            else if (node != null)
            {
                line = node.Location.Start.Line;
                message = s.Trim();
            }

            if (CurrentPath is null)
            {
                if (line < 0)
                {
                    return message;
                }
                else
                {
                    return $"Line {line}: {message}";
                }
            }
            else
            {
                var filename = LoosePath.GetFileName(CurrentPath);
                if (line < 0)
                {
                    return $"{filename}: {message}";
                }
                else
                {
                    return $"{filename}({line}): {message}";
                }
            }
        }


        internal WordNode CreateWordNode(string name)
        {
            return _commandHost.CreateWordNode(name);
        }
    }

}
