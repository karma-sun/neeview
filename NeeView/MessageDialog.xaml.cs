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
    /// UWP の UICommandモドキ。MessageDialog用
    /// </summary>
    public class UICommand
    {
        public string Label { get; set; }

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
        public static UICommand OK { get; } = new UICommand("OK");
        public static UICommand Yes { get; } = new UICommand("はい");
        public static UICommand No { get; } = new UICommand("いいえ");
        public static UICommand Cancel { get; } = new UICommand("キャンセル");
        public static UICommand Remove { get; } = new UICommand("削除する");
        public static UICommand Retry { get; } = new UICommand("リトライ");

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
        public new UICommand ShowDialog( )
        {
            return ShowDialog(null);
        }

        //
        private void InitializeButtons()
        {
            this.ButtonPanel.Children.Clear();

            if (Commands.Any())
            {
                var defaultComamnd = GetDefaultCommand();

                foreach (var command in Commands)
                {
                    var button = CreateButton(command, command == defaultComamnd);
                    this.ButtonPanel.Children.Add(button);
                }
            }
            else
            {
                var button = CreateButton(UICommands.OK, true);
                button.CommandParameter = null; // 設定されていなボタンなので結果がnullになるようにする
                this.ButtonPanel.Children.Add(button);
            }

            // Focus
            if (DefaultCommandIndex >= 0 && DefaultCommandIndex <this.ButtonPanel.Children.Count)
            {
                this.ButtonPanel.Children[DefaultCommandIndex].Focus();
            }
        }

        //
        private Button CreateButton(UICommand command, bool isDefault)
        {
            var button = new Button()
            {
                Content = command.Label,
                Command = ButtonClickedCommand,
                CommandParameter = command,
            };

            if (isDefault)
            {
                button.Foreground = Brushes.White;
                button.Background = Brushes.RoyalBlue;
            }

            return button;
        }


        /// <summary>
        /// ButtonClickedCommand command.
        /// </summary>
        public RelayCommand<UICommand> ButtonClickedCommand
        {
            get { return _ButtonClickedCommand = _ButtonClickedCommand ?? new RelayCommand<UICommand>(ButtonClickedCommand_Executed); }
        }

        //
        private RelayCommand<UICommand> _ButtonClickedCommand;

        //
        private void ButtonClickedCommand_Executed(UICommand command)
        {
            _resultCommand = command;
            this.DialogResult = true;
            this.Close();
        }
    }
}
