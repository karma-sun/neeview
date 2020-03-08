namespace NeeView
{
    public class OpenContextMenuCommand : CommandElement
    {
        public OpenContextMenuCommand() : base("OpenContextMenu")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenContextMenu;
            this.Note = Properties.Resources.CommandOpenContextMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.OpenContextMenu();
        }
    }
}
