namespace NeeView
{
    public class FocusBookmarkListCommand : CommandElement
    {
        public FocusBookmarkListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusBookmarkList;
            this.MenuText = Properties.Resources.CommandFocusBookmarkListMenu;
            this.Note = Properties.Resources.CommandFocusBookmarkListNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.FocusBookmarkList(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
