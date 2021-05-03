using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{

    public class CommandHost
    {
        private static Dictionary<string, object> _values = new Dictionary<string, object>();

        private object _sender;
        private CommandTable _commandTable;
        private ConfigMap _configMap;

        public CommandHost(object sender, CommandTable commandTable, ConfigMap configMap)
        {
            _sender = sender;
            _commandTable = commandTable;
            _configMap = configMap;
            Book = new BookAccessor();
            Command = new CommandAccessorMap(sender, commandTable);
            Bookshelf = new BookshelfPanelAccessor();
            PageList = new PageListPanelAccessor();
            Bookmark = new BookmarkPanelAccessor();
            Playlist = new PlaylistPanelAccessor();
            History = new HistoryPanelAccessor();
            Information = new InformationPanelAccessor();
            Effect = new EffectPanelAccessor();
            Navigator = new NavigatorPanelAccessor();
        }

        [WordNodeMember(IsAutoCollect = false)]
        public Dictionary<string, object> Values => _values;

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap Config => _configMap.Map;

        [WordNodeMember(IsAutoCollect = false)]
        public CommandAccessorMap Command { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookAccessor Book { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookshelfPanelAccessor Bookshelf { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public PageListPanelAccessor PageList { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkPanelAccessor Bookmark { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public PlaylistPanelAccessor Playlist { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public HistoryPanelAccessor History { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public InformationPanelAccessor Information { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public EffectPanelAccessor Effect { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public NavigatorPanelAccessor Navigator { get; }

        public object Pagemark
        {
            get => throw new NotSupportedException("Script: Pagemark is obsolete. Use PageList instead.");
        }


        [WordNodeMember]
        public void ShowMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
        }

        [WordNodeMember]
        public void ShowToast(string message)
        {
            ToastService.Current.Show(new Toast(message));
        }

        [WordNodeMember]
        public bool ShowDialog(string title, string message = "", int commands = 0)
        {
            return AppDispatcher.Invoke(() => ShowDialogIneer(title, message, commands));
        }

        private bool ShowDialogIneer(string title, string message, int commands)
        {
            var dialog = new MessageDialog(message, title);
            switch (commands)
            {
                default:
                    dialog.Commands.Add(UICommands.OK);
                    break;
                case 1:
                    dialog.Commands.Add(UICommands.OK);
                    dialog.Commands.Add(UICommands.Cancel);
                    break;
                case 2:
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    break;
            }
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result?.IsPositibe == true;
        }

        [WordNodeMember]
        public string ShowInputDialog(string title, string text = null)
        {
            return AppDispatcher.Invoke(() => ShowInputDialogIneer(title, text));
        }

        private string ShowInputDialogIneer(string title, string text)
        {
            var component = new InputDialogComponent(text);
            var dialog = new MessageDialog(component, title);
            dialog.Commands.Add(UICommands.OK);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result?.IsPositibe == true ? component.Text : null;
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            node.Children.Add(new WordNode(nameof(Values)));
            node.Children.Add(Config.CreateWordNode(nameof(Config)));
            node.Children.Add(Book.CreateWordNode(nameof(Book)));
            node.Children.Add(Command.CreateWordNode(nameof(Command)));
            node.Children.Add(Bookshelf.CreateWordNode(nameof(Bookshelf)));
            node.Children.Add(PageList.CreateWordNode(nameof(PageList)));
            node.Children.Add(Bookmark.CreateWordNode(nameof(Bookmark)));
            node.Children.Add(Playlist.CreateWordNode(nameof(Playlist)));
            node.Children.Add(History.CreateWordNode(nameof(History)));
            node.Children.Add(Information.CreateWordNode(nameof(Information)));
            node.Children.Add(Effect.CreateWordNode(nameof(Effect)));
            node.Children.Add(Navigator.CreateWordNode(nameof(Navigator)));

            return node;
        }

        
        private class InputDialogComponent : IMessageDialogContentComponent
        {
            private TextBox _textBox;

            public InputDialogComponent(string text)
            {
                _textBox = new TextBox() { Text = text ?? "", Padding = new Thickness(5.0) };
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            }

            private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                {
                    Decide?.Invoke(this, null);
                    e.Handled = true;
                }
            }

            public event EventHandler Decide;

            public object Content => _textBox;

            public string Text => _textBox.Text;

            public void OnLoaded(object sender, RoutedEventArgs e)
            {
                _textBox.Focus();
                _textBox.SelectAll();
            }
        }
    }
}
