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
    }
}
