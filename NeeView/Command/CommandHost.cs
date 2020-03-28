using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{

    public class CommandHost
    {
        private static Dictionary<string, object> _values = new Dictionary<string, object>();

        private CommandTable _commandTable;
        private ConfigMap _configMap;

        public CommandHost(CommandTable commandTable, ConfigMap configMap)
        {
            _commandTable = commandTable;
            _configMap = configMap;
            Book = new BookAccessor();
            Command = new CommandAccessorMap(_commandTable);
        }

        public Dictionary<string, object> Values => _values;

        public PropertyMap Config => _configMap.Map;

        public BookAccessor Book { get; }

        public CommandAccessorMap Command { get; }


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
