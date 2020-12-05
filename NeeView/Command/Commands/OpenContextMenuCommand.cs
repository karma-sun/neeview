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

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.OpenContextMenu();
        }
    }
}
