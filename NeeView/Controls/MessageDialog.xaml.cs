using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// ダイアログボタン配置
    /// </summary>
    public enum UICommandAlignment
    {
        Right,
        Left
    }

    /// <summary>
    /// UWP の UICommandモドキ。MessageDialog用
    /// </summary>
    public class UICommand
    {
        public string Label { get; set; }

        public UICommandAlignment Alignment { get; set; }

        public bool IsPositibe { get; set; }

        public UICommand(string label)
        {
            this.Label = label;
        }
    }

    /// <summary>
    /// UICommand の既定値集
    /// </summary>
    public static class UICommands
    {
        public static UICommand OK { get; } = new UICommand(Properties.Resources.WordOK) { IsPositibe = true };
        public static UICommand Yes { get; } = new UICommand(Properties.Resources.WordYes) { IsPositibe = true };
        public static UICommand No { get; } = new UICommand(Properties.Resources.WordNo);
        public static UICommand Cancel { get; } = new UICommand(Properties.Resources.WordCancel);
        public static UICommand Delete { get; } = new UICommand(Properties.Resources.WordDelete);
        public static UICommand Retry { get; } = new UICommand(Properties.Resources.WordRetry);

        // dialog.Commands.AddRange(...) のような使用を想定したセット
        public static List<UICommand> YesNo = new List<UICommand>() { Yes, No };
        public static List<UICommand> OKCancel = new List<UICommand>() { OK, Cancel };
    }

    /// <summary>
    /// ContentのDI
    /// </summary>
    public interface IMessageDialogContentComponent
    {
        event EventHandler Decide;

        object Content { get; }

        void OnLoaded(object sender, RoutedEventArgs e);
    }

    /// <summary>
    /// UWP の MessageDialogモドキ
    /// </summary>
    public partial class MessageDialog : Window, INotifyPropertyChanged
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


        private UICommand _resultCommand;


        public MessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Owner = MainWindow.Current;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.ShowInTaskbar = IsShowInTaskBar;
        }

        public MessageDialog(string message, string title) : this()
        {
            this.Title = title;
            this.Caption.Text = title;
            this.Message.Content = CreateTextContent(message);
        }

        public MessageDialog(FrameworkElement content, string title) : this()
        {
            this.Title = title;
            this.Caption.Text = title;
            this.Message.Content = content;
        }

        public MessageDialog(IMessageDialogContentComponent component, string title) : this()
        {
            this.Title = title;
            this.Caption.Text = title;
            this.Message.Content = component.Content;

            component.Decide += (s, e) => Decide();
            this.Loaded += (s, e) => component.OnLoaded(s, e);
        }


        public List<UICommand> Commands { get; private set; } = new List<UICommand>();

        public int DefaultCommandIndex { get; set; }

        public int CancelCommandIndex { get; set; } = -1;

        public static bool IsShowInTaskBar { get; set; } = true;


        private FrameworkElement CreateTextContent(string content)
        {
            return new TextBlock()
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private UICommand GetDefaultCommand()
        {
            return (DefaultCommandIndex >= 0 && DefaultCommandIndex < Commands.Count) ? Commands[DefaultCommandIndex] : null;
        }

        public UICommand ShowDialog(Window owner)
        {
            _resultCommand = null;

            InitializeButtons();

            if (owner != null)
            {
                this.Owner = owner;
            }

            return (base.ShowDialog() != null)
                ? _resultCommand
                : (CancelCommandIndex >= 0 && CancelCommandIndex < Commands.Count) ? Commands[CancelCommandIndex] : null;
        }

        private void Decide()
        {
            _resultCommand = Commands.FirstOrDefault(e => e.IsPositibe);
            this.DialogResult = true;
            this.Close();
        }

        public new UICommand ShowDialog()
        {
            return ShowDialog(null);
        }

        private void InitializeButtons()
        {
            this.ButtonPanel.Children.Clear();
            this.SubButtonPanel.Children.Clear();

            if (Commands.Any())
            {
                var defaultComamnd = GetDefaultCommand();

                foreach (var command in Commands)
                {
                    var button = CreateButton(command, command == defaultComamnd);
                    if (command.Alignment == UICommandAlignment.Left)
                    {
                        this.SubButtonPanel.Children.Add(button);
                    }
                    else
                    {
                        this.ButtonPanel.Children.Add(button);
                    }
                }
            }
            else
            {
                var button = CreateButton(UICommands.OK, true);
                button.CommandParameter = null; // 設定されていなボタンなので結果がnullになるようにする
                this.ButtonPanel.Children.Add(button);
            }

            // Focus
            if (DefaultCommandIndex >= 0 && DefaultCommandIndex < this.ButtonPanel.Children.Count)
            {
                this.ButtonPanel.Children[DefaultCommandIndex].Focus();
            }
        }

        private Button CreateButton(UICommand command, bool isDefault)
        {
            var button = new Button()
            {
                Style = App.Current.Resources["NVDialogButtonStyle"] as Style,
                Content = command.Label,
                Command = ButtonClickedCommand,
                CommandParameter = command,
            };

            if (isDefault)
            {
                button.Foreground = App.Current.Resources["NVDialogButtonForeground"] as Brush;
                button.Background = App.Current.Resources["NVDialogButtonBackground"] as Brush;
            }

            return button;
        }

        private void MesageDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// ButtonClickedCommand command.
        /// </summary>
        private RelayCommand<UICommand> _ButtonClickedCommand;
        public RelayCommand<UICommand> ButtonClickedCommand
        {
            get
            {
                return _ButtonClickedCommand = _ButtonClickedCommand ?? new RelayCommand<UICommand>(Execute);

                void Execute(UICommand command)
                {
                    _resultCommand = command;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }
    }
}
