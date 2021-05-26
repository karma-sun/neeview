using NeeView.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NeeView
{
    public class JavascriptEngine
    {
        private Jint.Engine _engine;
        private CommandHost _commandHost;
        private CancellationToken _cancellationToken;

        public JavascriptEngine()
        {
            _commandHost = new CommandHost(this, CommandTable.Current);
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

            var oldPath = CurrentPath;
            try
            {
                CurrentPath = path;
                var options = new Esprima.ParserOptions(path)
                {
                    AdaptRegexp = true,
                    Tolerant = true,
                    Loc = true,
                };
                var result = _engine.Execute(script, options).GetCompletionValue();
                return result?.ToObject();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ScriptException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ScriptException(CreateScriptErrorMessage(ex.Message), ex);
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

            var message = (ex is ScriptException) ? ex.Message : CreateScriptErrorMessage(ex.Message).ToString();
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


        public ScriptNotice CreateScriptErrorMessage(string s)
        {
            // NOTE: GetLastSyntaxNode() はバージョンによっては使用できないので注意
            var node = _engine.GetLastSyntaxNode();

            string source = null;
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
                source = node.Location.Source;
                line = node.Location.Start.Line;
                message = s.Trim();
            }

            return new ScriptNotice(source, line, message);
        }



        internal WordNode CreateWordNode(string name)
        {
            return _commandHost.CreateWordNode(name);
        }
    }

}
