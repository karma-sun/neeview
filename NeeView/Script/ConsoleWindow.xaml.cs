using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ConsoleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        public static ConsoleWindow Current { get; private set; }

        public ConsoleWindow()
        {
            InitializeComponent();
            Current = this;
            this.Closed += (s, e) => Current = null;
            this.Console.ConsoleHost = new ConsoleHost(this);
        }
    }

    public class ConsoleHost : IConsoleHost
    {
        private Window _owner;
        private JavascriptEngine _engine;

        public ConsoleHost(Window owner)
        {
            _owner = owner;

            var host = new CommandHost(this, CommandTable.Current, ConfigMap.Current);

            _engine = new JavascriptEngine(host);
            _engine.CurrentPath = Config.Current.Script.GetCurrentScriptFolder();
            _engine.LogAction = e => Output?.Invoke(this, new ConsoleHostOutputEventArgs(ToJavascriptString(e)));

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
                    host.CreateWordNode("nv"),
                },
            };

            WordTree = new WordTree(wordTreeRoot);
        }

        public event EventHandler<ConsoleHostOutputEventArgs> Output;

        public WordTree WordTree { get; set; }

        public string Execute(string input, CancellationToken token)
        {
            switch (input.Trim())
            {
                case "?":
                case "help":
                    new ScriptManual().OpenScriptManual();
                    return null;

                case "exit":
                    _owner.Close();
                    return null;

                default:
                    try
                    {
                        var result = _engine.Execute(input, token);
                        return ToJavascriptString(result);
                    }
                    catch (Exception ex)
                    {
                        return GetExceptionMessage(ex);
                    }
                    finally
                    {
                        CommandTable.Current.FlushInputGesture();
                    }
            }

            string GetExceptionMessage(Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return ex.Message + " " + GetExceptionMessage(ex.InnerException);
                }
                else
                {
                    return ex.Message;
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
