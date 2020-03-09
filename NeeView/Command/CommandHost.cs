using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class CommandAccessor
    {
        private CommandElement _command;

        public CommandAccessor(CommandElement command)
        {
            _command = command;
        }

        public bool Execute(IDictionary<string, object> args = null)
        {
            var param = _command.CreateOverwriteCommandParameter(args);
            if (_command.CanExecute(param))
            {
                _command.Execute(param);
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public class CommandHost
    {
        private CommandTable _commandTable;

        public CommandHost(CommandTable commandTable)
        {
            _commandTable = commandTable;
        }

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

        // sample.
        public string GetBookName()
        {
            return "吾輩は猫である";
        }
    }

}
