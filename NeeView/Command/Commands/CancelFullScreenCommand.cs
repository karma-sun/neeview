namespace NeeView
{
    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandCancelFullScreen;
            this.Note = Properties.Resources.CommandCancelFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.SetFullScreen(sender, false);
        }
    }
}
