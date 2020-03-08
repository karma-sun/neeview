namespace NeeView
{
    public class HelpSearchOptionCommand : CommandElement
    {
        public HelpSearchOptionCommand() : base("HelpSearchOption")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpSearchOption;
            this.MenuText = Properties.Resources.CommandHelpSearchOptionMenu;
            this.Note = Properties.Resources.CommandHelpSearchOptionNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MenuBar.Current.OpenSearchOptionHelp();
        }
    }
}
