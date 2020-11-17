using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var host = new CommandHost(CommandTable.Current, ConfigMap.Current);

            _engine = new JavascriptEngine(host);
            _engine.CurrentPath = Config.Current.Script.GetCurrentScriptFolder();
            _engine.LogAction = e => Output?.Invoke(this, new ConsoleHostOutputEventArgs(ToJavascriptString(e, false)));

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
                    CommandTable.Current.OpenScriptHelp();
                    return null;

                case "exit":
                    _owner.Close();
                    return null;

                default:
                    try
                    {
                        var result = _engine.Execute(input, token);
                        return ToJavascriptString(result, false);
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

        private static string ToJavascriptString(object source, bool isNest)
        {
            if (source is null)
            {
                return "null";
            }
            else if (source is Enum enm)
            {
                return Convert.ToInt32(enm).ToString();
            }
            else if (source is bool boolean)
            {
                return boolean ? "true" : "false";
            }
            else if (source is string str)
            {
                return isNest ? "\"" + str + "\"" : str;
            }
            else if (source is object[] objects)
            {
                return "[" + string.Join(", ", objects.Select(e => ToJavascriptString(e, true))) + "]";
            }
            else if (source is IDictionary<string, object> dic)
            {
                return "{" + string.Join(", ", dic.Select(e => "\"" + e.Key + "\": " + ToJavascriptString(e.Value, true))) + "}";
            }
            else
            {
                return source?.ToString();
            }
        }
    }
}
