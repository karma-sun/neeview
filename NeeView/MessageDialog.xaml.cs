// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
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
    /// UWP の UICommandモドキ
    /// </summary>
    public class UICommand
    {
        public string Label { get; set; }

        public UICommand(string label)
        {
            this.Label = label;
        }
    }

    public static class UICommands
    {
        public static UICommand OK { get; } = new UICommand("OK");
        public static UICommand Yes { get; } = new UICommand("はい");
        public static UICommand No { get; } = new UICommand("いいえ");
        public static UICommand Cancel { get; } = new UICommand("キャンセル");
        public static UICommand Remove { get; } = new UICommand("削除する");
        public static UICommand Retry { get; } = new UICommand("リトライ");

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
                

        public List<UICommand> Commands { get; private set; } = new List<UICommand>();

        //
        public MessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Owner = App.Current?.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        //
        public MessageDialog(string message, string title) : this()
        {
            this.Title = title;
            this.Message.Content = CreateTextContent(message);
        }

        //
        public MessageDialog(FrameworkElement content, string title) : this()
        {
            this.Title = title;
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
        public int DefaultCommandIndex { get; set; }

        //
        private UICommand GetDefaultCommand()
        {
            return (DefaultCommandIndex >= 0 && DefaultCommandIndex < Commands.Count) ? Commands[DefaultCommandIndex] : null;
        }

        //
        public int CancelCommandIndex { get; set; } = -1;

        //
        private UICommand _resultCommand;

        //
        public new UICommand ShowDialog()
        {
            _resultCommand = null;

            InitializeButtons();

            return (base.ShowDialog() != null)
                ? _resultCommand
                : (CancelCommandIndex >= 0 && CancelCommandIndex < Commands.Count) ? Commands[CancelCommandIndex] : null;
        }

        //
        public async Task<UICommand> ShowDialogAsync()
        {
            await Task.Yield();
            return ShowDialog();
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
                    var button = new Button()
                    {
                        Content = command.Label,
                        Command = ButtonClickedCommand,
                        CommandParameter = command,
                    };

                    if (command == defaultComamnd)
                    {
                        button.Foreground = Brushes.White;
                        button.Background = Brushes.RoyalBlue;
                    }

                    this.ButtonPanel.Children.Add(button);
                }
            }
            else
            {
                var button = new Button()
                {
                    Content = "OK",
                    Command = ButtonClickedCommand,
                    Foreground = Brushes.White,
                    Background = Brushes.RoyalBlue,
                };

                this.ButtonPanel.Children.Add(button);
            }

            // Focus
            if (DefaultCommandIndex >= 0 && DefaultCommandIndex <this.ButtonPanel.Children.Count)
            {
                this.ButtonPanel.Children[DefaultCommandIndex].Focus();
            }
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
