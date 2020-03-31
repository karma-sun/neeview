// from https://stackoverflow.com/questions/14948171/how-to-emulate-a-console-in-wpf
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
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

        public readonly static RoutedCommand ClearScreenCommand = new RoutedCommand("ClearScreen", typeof(ConsoleEmulator), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.L, ModifierKeys.Control) }));

        private string _consoleInput = string.Empty;
        private List<string> _history = new List<string>();
        private int _historyIndex;
        private List<string> _candidates;
        private int _candidatesIndex;

        public ConsoleEmulator()
        {
            InitializeComponent();
            Scroller.DataContext = this;

            this.Loaded += ConsoleEmulator_Loaded;
            this.RootPanel.MouseDown += RootPanel_MouseDown;
            this.OutputBlock.PreviewKeyDown += OutputBlock_PreviewKeyDown;
            this.InputBlock.Loaded += InputBlock_Loaded;
            this.InputBlock.PreviewKeyDown += InputBlock_PreviewKeyDown;

            this.CommandBindings.Add(new CommandBinding(ClearScreenCommand, ClearScreen, (s, e) => e.CanExecute = true));
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
                    control.WriteLine(args.Output ?? string.Empty);
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


        private void ConsoleEmulator_Loaded(object sender, RoutedEventArgs e)
        {
            if (FirstMessage != null)
            {
                WriteLine(FirstMessage);
            }
        }

        private void RootPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.InputBlock.Focus();
            e.Handled = true;
        }


        private void OutputBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            FocusToInputBlock();
        }

        // NOTE: 未使用
        private void OutputBlock_PreviewMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.OutputBlock.SelectionLength == 0)
            {
                FocusToInputBlock();
            }
        }

        private void InputBlock_Loaded(object sender, RoutedEventArgs e)
        {
            this.InputBlock.Focus();
        }


        private void InputBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.System)
            {
                return;
            }

            // TAB interpolation
            if (e.Key == Key.Tab && ConsoleHost.WordTree != null)
            {
                if (_candidates == null)
                {
                    _candidates = ConsoleHost.WordTree.Interpolate(ConsoleInput).OrderBy(x => x).ToList();
                    _candidatesIndex = 0;
                }
                else
                {
                    var direction = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1;
                    _candidatesIndex = (_candidatesIndex + _candidates.Count + direction) % _candidates.Count;
                }
                ConsoleInput = _candidates[_candidatesIndex];
                FocusToInputBlock();
                e.Handled = true;
                return;
            }

            _candidates = null;

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

        private void ClearScreen(object sender, ExecutedRoutedEventArgs e)
        {
            this.OutputBlock.Clear();
            this.OutputBlock.Visibility = Visibility.Collapsed;
            this.InputBlock.Text = ConsoleInput = string.Empty;
            this.InputBlock.Focus();
        }

        private void WriteLine(string text)
        {
            //if (string.IsNullOrEmpty(text)) return;

            if (string.IsNullOrEmpty(this.OutputBlock.Text))
            {
                this.OutputBlock.AppendText(text);
            }
            else
            {
                this.OutputBlock.AppendText("\r\n" + text);
            }

            this.OutputBlock.Visibility = Visibility.Visible;
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

            WriteLine(Prompt + ConsoleInput);
            ConsoleInput = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            switch (input.Trim())
            {
                case "cls":
                    ClearScreen(this, null);
                    break;

                default:
                    var result = ConsoleHost?.Execute(input);
                    WriteLine(result);
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

        WordTree WordTree { get; set; }

        string Execute(string input);
    }
}
