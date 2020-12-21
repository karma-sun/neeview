namespace NeeView
{
    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MenuBar.Current.OpenMainMenuHelp();
        }
    }
}
