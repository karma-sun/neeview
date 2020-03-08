namespace NeeView
{
    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand() : base("HelpMainMenu")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpMainMenu;
            this.MenuText = Properties.Resources.CommandHelpMainMenuMenu;
            this.Note = Properties.Resources.CommandHelpMainMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MenuBar.Current.OpenMainMenuHelp();
        }
    }
}
