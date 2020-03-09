// from https://stackoverflow.com/questions/14948171/how-to-emulate-a-console-in-wpf
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    public partial class ConsoleEmulator : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private string _consoleInput = string.Empty;
        private ObservableCollection<string> _consoleOutput = new ObservableCollection<string>();
        private List<string> _history = new List<string>();
        private int _historyIndex;


        public ConsoleEmulator()
        {
            InitializeComponent();
            Scroller.DataContext = this;

            this.Loaded += ConsoleEmulator_Loaded;
            this.GotFocus += ConsoleEmulator_GotFocus;
            this.InputBlock.PreviewKeyDown += InputBlock_PreviewKeyDown;
        }


        public IConsoleHost ConsoleHost
        {
            get { return (IConsoleHost)GetValue(ConsoleHostProperty); }
            set { SetValue(ConsoleHostProperty, value); }
        }

        public static readonly DependencyProperty ConsoleHostProperty =
            DependencyProperty.Register("ConsoleHost", typeof(IConsoleHost), typeof(ConsoleEmulator), new PropertyMetadata(null, ConsoleHostChanged));

        private static void ConsoleHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConsoleEmulator control)
            {
                if (e.OldValue != null)
                {
                    ((IConsoleHost)e.OldValue).Output -= Log;
                }
                if (control.ConsoleHost != null)
                {
                    control.ConsoleHost.Output += Log;
                }

                void Log(object sender, ConsoleHostOutputEventArgs args)
                {
                    control.ConsoleOutput.Add(args.Output ?? string.Empty);
                }
            }
        }

        public string Prompt
        {
            get { return (string)GetValue(PromptProperty); }
            set { SetValue(PromptProperty, value); }
        }

        public static readonly DependencyProperty PromptProperty =
            DependencyProperty.Register("Prompt", typeof(string), typeof(ConsoleEmulator), new PropertyMetadata("> "));


        public string FirstMessage
        {
            get { return (string)GetValue(FirstMessageProperty); }
            set { SetValue(FirstMessageProperty, value); }
        }

        public static readonly DependencyProperty FirstMessageProperty =
            DependencyProperty.Register("FirstMessage", typeof(string), typeof(ConsoleEmulator), new PropertyMetadata(null));


        public string ConsoleInput
        {
            get => _consoleInput;
            set => SetProperty(ref _consoleInput, value);
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get => _consoleOutput;
            set => SetProperty(ref _consoleOutput, value);
        }


        private void ConsoleEmulator_GotFocus(object sender, RoutedEventArgs e)
        {
            InputBlock.Focus();
        }

        private void ConsoleEmulator_Loaded(object sender, RoutedEventArgs e)
        {
            if (FirstMessage != null)
            {
                ConsoleOutput.Add(FirstMessage);
            }
        }

        private void InputBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConsoleInput = InputBlock.Text;
                RunCommand();
                FocusToInputBlock();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                PreviewHistory();
                FocusToInputBlock();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                NextHistory();
                FocusToInputBlock();
                e.Handled = true;
            }
        }

        private void FocusToInputBlock()
        {
            this.InputBlock.Focus();
            this.Scroller.ScrollToBottom();
            this.Scroller.ScrollToLeftEnd();
            this.InputBlock.Select(InputBlock.Text.Length, 0);
        }

        private void PreviewHistory()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
            }

            if (_history.Count > 0)
            {
                ConsoleInput = _history[_historyIndex];
            }
        }

        private void NextHistory()
        {
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
            }

            if (_historyIndex < _history.Count)
            {
                ConsoleInput = _history[_historyIndex];
            }
        }

        private void RunCommand()
        {
            var input = ConsoleInput;

            ConsoleOutput.Add(Prompt + ConsoleInput);
            ConsoleInput = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            switch (input.Trim())
            {
                case "cls":
                    ConsoleOutput.Clear();
                    break;
                case "exit":
                    ConsoleHost?.Close();
                    break;
                default:
                    var result = ConsoleHost?.Execute(input);
                    ConsoleOutput.Add(result);
                    break;
            }

            _history.Add(input);
            _historyIndex = _history.Count;
        }
    }


    public class ConsoleHostOutputEventArgs : EventArgs
    {
        public ConsoleHostOutputEventArgs(string output)
        {
            Output = output;
        }

        public string Output { get; set; }
    }

    public interface IConsoleHost
    {
        event EventHandler<ConsoleHostOutputEventArgs> Output;

        void Close();

        string Execute(string input);
    }
}
