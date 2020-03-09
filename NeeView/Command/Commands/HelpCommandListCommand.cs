namespace NeeView
{
    public class HelpCommandListCommand : CommandElement
    {
        public HelpCommandListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpCommandList;
            this.MenuText = Properties.Resources.CommandHelpCommandListMenu;
            this.Note = Properties.Resources.CommandHelpCommandListNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            CommandTable.Current.OpenCommandListHelp();
        }
    }
}
