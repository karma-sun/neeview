namespace NeeView
{
    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpMainMenu;
            this.MenuText = Properties.Resources.CommandHelpMainMenuMenu;
            this.Note = Properties.Resources.CommandHelpMainMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MenuBar.Current.OpenMainMenuHelp();
        }
    }
}
