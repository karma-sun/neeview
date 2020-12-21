namespace NeeView
{
    public class FocusFolderSearchBoxCommand : CommandElement
    {
        public FocusFolderSearchBoxCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusBookshelfSearchBox(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
