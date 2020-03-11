using System;
using System.Linq;

namespace NeeView
{
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
    }
}
