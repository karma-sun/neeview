﻿using System;
using System.Collections.Generic;
using System.IO;

namespace NeeView
{
    public class JavascriptEngine
    {
        private Jint.Engine _engine;
        private CommandHost _commandHost;

        public JavascriptEngine(CommandHost commandHost)
        {
            _commandHost = commandHost;

            _engine = new Jint.Engine();
            _engine.SetValue("log", (Action<object>)Log);
            _engine.SetValue("include", (Func<string, object>)ExecureFile);
            _engine.SetValue("nv", _commandHost);
        }

        public string CurrentPath { get; set; }

        public Action<object> LogAction { get; set; } = Console.WriteLine;

        public void Log(object log)
        {
            LogAction?.Invoke(log);
        }

        public object Execute(string script)
        {
            var result = _engine.Execute(script).GetCompletionValue();
            return result?.ToObject();
        }

        public object ExecureFile(string path)
        {
            var fullpath = GetFullPath(path);
            string script = File.ReadAllText(fullpath);

            var oldPath = CurrentPath;
            try
            {
                CurrentPath = Path.GetDirectoryName(fullpath);
                return Execute(script);
            }
            finally
            {
                CurrentPath = oldPath;
            }
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
