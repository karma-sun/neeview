namespace NeeView
{
    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.SetFullScreen(sender, false);
        }
    }
}
