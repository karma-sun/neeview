namespace NeeView
{
    public class OpenContextMenuCommand : CommandElement
    {
        public OpenContextMenuCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.OpenContextMenu();
        }
    }
}
