namespace NeeView
{
    public class HelpSearchOptionCommand : CommandElement
    {
        public HelpSearchOptionCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpSearchOption;
            this.MenuText = Properties.Resources.CommandHelpSearchOptionMenu;
            this.Note = Properties.Resources.CommandHelpSearchOptionNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MenuBar.Current.OpenSearchOptionHelp();
        }
    }
}
