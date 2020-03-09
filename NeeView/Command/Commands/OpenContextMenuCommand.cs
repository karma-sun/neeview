namespace NeeView
{
    public class OpenContextMenuCommand : CommandElement
    {
        public OpenContextMenuCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenContextMenu;
            this.Note = Properties.Resources.CommandOpenContextMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MainWindow.Current.OpenContextMenu();
        }
    }
}
