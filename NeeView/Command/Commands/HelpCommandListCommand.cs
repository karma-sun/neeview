namespace NeeView
{
    public class HelpCommandListCommand : CommandElement
    {
        public HelpCommandListCommand() : base(CommandType.HelpCommandList)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpCommandList;
            this.MenuText = Properties.Resources.CommandHelpCommandListMenu;
            this.Note = Properties.Resources.CommandHelpCommandListNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            CommandTable.Current.OpenCommandListHelp();
        }
    }
}
