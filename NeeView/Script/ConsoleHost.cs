using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace NeeView
{
    public class ConsoleHost : IConsoleHost 
    {
        private Window _owner;
        private JavascriptEngine _engine;
        private WordTree _wordTree;


        public ConsoleHost(Window owner)
        {
            _owner = owner;
            UpdateEngine();
        }


#pragma warning disable CS0067
        public event EventHandler<ConsoleHostOutputEventArgs> Output;
#pragma warning restore CS0067


        public WordTree WordTree
        {
            get
            {
                UpdateEngine();
                return _wordTree;
            }
        }


        private void UpdateEngine()
        {
            if (_engine != null && !_engine.IsDarty) return;

            var engine = new JavascriptEngine();
            engine.CurrentFolder = Config.Current.Script.ScriptFolder;

            var wordTreeRoot = new WordNode()
            {
                Children = new List<WordNode>()
                {
                    new WordNode("cls"),
                    new WordNode("help"),
                    new WordNode("exit"),
                    new WordNode("log"),
                    new WordNode("system"),
                    new WordNode("include"),
                    engine.CreateWordNode("nv"),
                },
            };

            _engine = engine;
            _wordTree = new WordTree(wordTreeRoot);
        }

        public string Execute(string input, CancellationToken token)
        {
            switch (input.Trim())
            {
                case "?":
                case "help":
                    new ScriptManual().OpenScriptManual();
                    return null;

                case "exit":
                    AppDispatcher.Invoke(() => _owner.Close());
                    return null;

                default:
                    UpdateEngine();
                    JavascroptEngineMap.Current.Add(_engine);
                    try
                    {
                        var result = _engine.Execute(null, input, token);
                        return ToJavascriptString(result);
                    }
                    catch (Exception ex)
                    {
                        _engine.ExceptionPrcess(ex);
                        return null;
                    }
                    finally
                    {
                        JavascroptEngineMap.Current.Remove(_engine);
                        CommandTable.Current.FlushInputGesture();
                    }
            }
        }

        private static string ToJavascriptString(object source)
        {
            var builder = new JsonStringBulder();
            return builder.AppendObject(source).ToString();
        }
    }
}
