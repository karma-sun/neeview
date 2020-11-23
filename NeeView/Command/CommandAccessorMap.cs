using System;
using System.Collections.Generic;

namespace NeeView
{
    public class CommandAccessorMap
    {
        private object _sender;
        private CommandTable _commandTable;

        public CommandAccessorMap(object sender, CommandTable commandTable)
        {
            _sender = sender;
            _commandTable = commandTable;
        }

        public CommandAccessor this[string name]
        {
            get
            {
                if (_commandTable.TryGetValue(name, out CommandElement command))
                {
                    return new CommandAccessor(_sender, command);
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
