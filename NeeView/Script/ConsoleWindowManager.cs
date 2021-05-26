using NeeLaboratory.Collection;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class ConsoleWindowManager
    {
        static ConsoleWindowManager() => Current = new ConsoleWindowManager();
        public static ConsoleWindowManager Current { get; }

        private ConsoleWindowManager()
        {
        }


        const int _messagesCapacity = 256;
        private ConsoleWindow _window;
        private FixedQueue<string> _messages = new FixedQueue<string>(_messagesCapacity);


        public bool IsOpened => _window != null;


        public ConsoleWindow OpenWindow()
        {
            if (_window != null)
            {
                AppDispatcher.Invoke(() => _window.Activate());
            }
            else
            {
                AppDispatcher.Invoke(() =>
                {
                    _window = new ConsoleWindow();
                    _window.Owner = App.Current.MainWindow;
                    _window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    _window.Show();
                    _window.Closed += (s, e) => _window = null;
                });

                Flush();
            }

            return _window;
        }

        public void InforMessage(string message, bool withToast)
        {
            WriteLine(ScriptMessageLevel.Info, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, Properties.Resources.ScriptErrorDialog_Title_Info, ToastIcon.Information, Properties.Resources.ScriptErrorDialog_OpenConsole, () => OpenWindow()));
            }
        }

        public void WarningMessage(string message, bool withToast)
        {
            WriteLine(ScriptMessageLevel.Warning, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, Properties.Resources.ScriptErrorDialog_Title_Warning, ToastIcon.Warning, Properties.Resources.ScriptErrorDialog_OpenConsole, () => OpenWindow()));
            }
        }

        public void ErrorMessage(string message, bool withToast)
        {
            WriteLine(ScriptMessageLevel.Error, message);

            if (withToast)
            {
                ToastService.Current.Show("ScriptNotice", new Toast(message, Properties.Resources.ScriptErrorDialog_Title_Error, ToastIcon.Error, Properties.Resources.ScriptErrorDialog_OpenConsole, () => OpenWindow()));
            }
        }

        public void WriteLine(ScriptMessageLevel level, string message)
        {
            var fixedMessage = (level != ScriptMessageLevel.None) ? level.ToString() + ": " + message : message;
            Debug.WriteLine(fixedMessage);

            if (_window != null)
            {
                _window.Console.WriteLine(fixedMessage);
            }
            else
            {
                _messages.Enqueue(fixedMessage);
            }
        }

        public void Flush()
        {
            if (_messages.Count == 0) return;

            var messages = _messages;
            _messages = new FixedQueue<string>(_messagesCapacity);

            var console = _window?.Console;
            if (console != null)
            {
                foreach (var message in messages)
                {
                    console.WriteLine(message);
                }
            }
        }
    }
}
