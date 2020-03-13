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

        public bool ShowDialog(string title, string message = "", int commands = 0)
        {
            var dialog = new MessageDialog(message, title);

            switch (commands)
            {
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
    }
}
