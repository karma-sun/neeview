namespace NeeView
{
    public class FocusFolderSearchBoxCommand : CommandElement
    {
        public FocusFolderSearchBoxCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusFolderSearchBox;
            this.MenuText = Properties.Resources.CommandFocusFolderSearchBoxMenu;
            this.Note = Properties.Resources.CommandFocusFolderSearchBoxNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusBookshelfSearchBox(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
