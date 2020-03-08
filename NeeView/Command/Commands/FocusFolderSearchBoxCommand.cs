namespace NeeView
{
    public class FocusFolderSearchBoxCommand : CommandElement
    {
        public FocusFolderSearchBoxCommand() : base("FocusFolderSearchBox")
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusFolderSearchBox;
            this.MenuText = Properties.Resources.CommandFocusFolderSearchBoxMenu;
            this.Note = Properties.Resources.CommandFocusFolderSearchBoxNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.FocusFolderSearchBox(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
