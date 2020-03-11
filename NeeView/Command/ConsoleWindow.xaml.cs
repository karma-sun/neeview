using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            this.Console.Loaded += (s, e) => ((UIElement)s).Focus();
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

            var host = new CommandHost(CommandTable.Current);

            _engine = new JavascriptEngine(host);
            _engine.CurrentPath = CommandTable.Current.ScriptFolder;
            _engine.LogAction = e => Output?.Invoke(this, new ConsoleHostOutputEventArgs(e?.ToString()));
        }

        public event EventHandler<ConsoleHostOutputEventArgs> Output;

        public void Close()
        {
            _owner.Close();
        }

        public string Execute(string input)
        {
            try
            {
                var result = _engine.Execute(input);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                CommandTable.Current.FlushInputGesture();
            }
        }
    }
}
