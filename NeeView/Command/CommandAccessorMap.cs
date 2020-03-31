using System;
using System.Collections.Generic;

namespace NeeView
{
    public class CommandAccessorMap
    {
        private CommandTable _commandTable;

        public CommandAccessorMap(CommandTable commandTable)
        {
            _commandTable = commandTable;
        }

        public CommandAccessor this[string name]
        {
            get
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
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();
            foreach(var commandName in _commandTable.Keys)
            {
                var commandAccessor = this[commandName];
                if (commandAccessor != null)
                {
                    node.Children.Add(commandAccessor.CreateWordNode(commandName));
                }
            }
            return node;
        }
    }
}
