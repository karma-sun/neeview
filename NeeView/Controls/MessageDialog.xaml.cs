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
        public static UICommand OK { get; } = new UICommand(Properties.Resources.WordOK);
        public static UICommand Yes { get; } = new UICommand(Properties.Resources.WordYes);
        public static UICommand No { get; } = new UICommand(Properties.Resources.WordNo);
        public static UICommand Cancel { get; } = new UICommand(Properties.Resources.WordCancel);
        public static UICommand Delete { get; } = new UICommand(Properties.Resources.WordDelete);
        public static UICommand Retry { get; } = new UICommand(Properties.Resources.WordRetry);

        // dialog.Commands.AddRange(...) のような使用を想定したセット
        public static List<UICommand> YesNo = new List<UICommand>() { Yes, No };
        public static List<UICommand> OKCancel = new List<UICommand>() { OK, Cancel };
    }


    /// <summary>
    /// UWP の MessageDialogモドキ
    /// </summary>
    public partial class MessageDialog : Window, INotifyPropertyChanged
    {
        // PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //
        public List<UICommand> Commands { get; private set; } = new List<UICommand>();

        //
        public int DefaultCommandIndex { get; set; }

        //
        public int CancelCommandIndex { get; set; } = -1;

        //
        private UICommand _resultCommand;

        //
        public static bool IsShowInTaskBar { get; set; } = true;

        //
        public MessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Owner = MainWindow.Current;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.ShowInTaskbar = IsShowInTaskBar;
        }

        //
        public MessageDialog(string message, string title) : this()
        {
            this.Title = title;
            this.Caption.Text = title;
            this.Message.Content = CreateTextContent(message);
        }

        //
        public MessageDialog(FrameworkElement content, string title) : this()
        {
            this.Title = title;
            this.Caption.Text = title;
            this.Message.Content = content;
        }

        //
        private FrameworkElement CreateTextContent(string content)
        {
            return new TextBlock()
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap
            };
        }

        //
        private UICommand GetDefaultCommand()
        {
            return (DefaultCommandIndex >= 0 && DefaultCommandIndex < Commands.Count) ? Commands[DefaultCommandIndex] : null;
        }

        //
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

        //
        public new UICommand ShowDialog()
        {
            return ShowDialog(null);
        }

        //
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

        //
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

        //
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
