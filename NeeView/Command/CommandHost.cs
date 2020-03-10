using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        public string Address => Book()?.Address;

        public bool IsMedia => Book()?.IsMedia == true;

        public bool IsNew => Book()?.IsNew == true;

        private Book Book() => BookOperation.Current.Book;
    }


    /// <summary>
    /// コマンドアクセス
    /// </summary>
    public class CommandAccessor
    {
        private CommandElement _command;
        private IDictionary<string, object> _patch;

        public CommandAccessor(CommandElement command)
        {
            _command = command;
        }

        public bool Execute(object arg = null)
        {
            var param = _command.CreateOverwriteCommandParameter(_patch);
            if (_command.CanExecute(param, arg, CommandOption.None))
            {
                _command.Execute(param, arg, CommandOption.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            if (_patch == null)
            {
                _patch = patch;
            }
            else
            {
                foreach (var pair in patch)
                {
                    _patch[pair.Key] = pair.Value;
                }
            }

            return this;
        }
    }


    public class CommandHost
    {
        private CommandTable _commandTable;

        public CommandHost(CommandTable commandTable)
        {
            _commandTable = commandTable;
        }


        public BookAccessor Book { get; } = new BookAccessor();

        public CommandAccessor Command(string name)
        {
            if (_commandTable.TryGetValue(name, out CommandElement command))
            {
                return new CommandAccessor(command);
            }
            else
            {
                return null;
            }
        }

        public void ShowMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
        }

        public void ShowToast(string message)
        {
            ToastService.Current.Show(new Toast(message));
        }

        public bool ShowDialog(string message, string title, string commands = "OK")
        {
            var dialog = new MessageDialog(message, title);

            switch (commands)
            {
                case "YesNo":
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    break;
                case "OKCancel":
                    dialog.Commands.Add(UICommands.OK);
                    dialog.Commands.Add(UICommands.Cancel);
                    break;
            }

            var result = dialog.ShowDialog(App.Current.MainWindow);
            return (result == UICommands.Yes || result == UICommands.OK);
        }

        // sample.
        public string GetBookName()
        {
            return "吾輩は猫である";
        }
    }

}
