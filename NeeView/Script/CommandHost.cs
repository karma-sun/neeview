using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            Pagemark = new PagemarPanelAccessor();
            History = new HistoryPanelAccessor();
        }

        public Dictionary<string, object> Values => _values;

        public PropertyMap Config => _configMap.Map;

        public CommandAccessorMap Command { get; }

        public BookAccessor Book { get; }
        public BookshelfPanelAccessor Bookshelf { get; }
        public PageListPanelAccessor PageList { get; }
        public BookmarkPanelAccessor Bookmark { get; }
        public  PagemarPanelAccessor Pagemark { get; }
        public HistoryPanelAccessor History { get; }


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
            return (result == UICommands.Yes || result == UICommands.OK);
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
            node.Children.Add(Pagemark.CreateWordNode(nameof(Pagemark)));
            node.Children.Add(History.CreateWordNode(nameof(History)));
            

            return node;
        }
    }

}
